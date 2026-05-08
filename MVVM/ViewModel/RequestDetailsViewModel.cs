using Azure.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Telecom.Utilities;
using Telecom_ThesisProject.Core;
using Telecom_ThesisProject.MVVM.Model;
using Telecom_ThesisProject.Services;
using Telecom_ThesisProject.Utilities.DTOs;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.MVVM.ViewModel
{
    public class RequestDetailsViewModel : ObservableObject
    {
        private readonly RequestService _requestService;
        private readonly IMessageService _messageService;

        #region Properties

        public string PageTitle { get; private set; } = "Заявка";

        private Model.Request? _request;
        public Model.Request? Request
        {
            get => _request;
            set 
            { 
                _request = value;
                OnPropertyChanged();
            }
        }

        private string? _commentText;
        public string? CommentText
        {
            get => _commentText;
            set
            {
                _commentText = value;
                OnPropertyChanged();

                if (string.IsNullOrWhiteSpace(_commentText))
                {
                    CommentTitle = null;
                    _selectedRequestComment = null;
                    OnPropertyChanged(nameof(SelectedRequestComment));
                }

                AddCommentCommand.RaiseCanExecuteChanged();
            }
        }

        private string? _commentTitle;

        public string? CommentTitle
        {
            get => _commentTitle;
            set
            {
                _commentTitle = value;
                OnPropertyChanged();
            }
        }


        private RequestComment? _selectedRequestComment;
        public RequestComment? SelectedRequestComment
        {
            get => _selectedRequestComment;
            set
            {
                _selectedRequestComment = value;
                OnPropertyChanged();

                if (SelectedRequestComment != null)
                {
                    CommentTitle = value?.CommentTitle;
                    
                    EditCommentManually(value);
                    AddCommentCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private int? _selectedStatusId;
        public int? SelectedStatusId
        {
            get => _selectedStatusId;
            set
            {
                _selectedStatusId = value;
                OnPropertyChanged();
            }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage; 
            set 
            {
                _errorMessage = value; 
                OnPropertyChanged();
            }
        }

        #endregion

        #region Observable Collections

        private ObservableCollection<RequestComment> _requestComments = [];
        public ObservableCollection<RequestComment> RequestComments
        {
            get => _requestComments;
            set
            {
                _requestComments = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RequestStatus> _requestStatuses = [];
        public ObservableCollection<RequestStatus> RequestStatuses
        {
            get => _requestStatuses;
            set
            {
                _requestStatuses = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region RelayCommands

        public RelayCommand GoBackCommand { get; }
        public RelayCommand ClientDetailsCommand {  get; }
        public RelayCommand AddCommentCommand { get; }
        public RelayCommand EditCommentCommand { get; }
        public RelayCommand RemoveCommentCommand { get; }
        public RelayCommand SetRequestStatusCommand { get; }
        public RelayCommand AssignedToRequestCommand { get; }
        public RelayCommand EditCommand { get; }

        #endregion

        #region Actions

        public Action? GoBack { get; set; }
        public Action<Client?>? NavigateToClientDetails { get; set; }
        public Action<Model.Request?>? NavigateToAddEditPage { get; set; }

        #endregion

        public RequestDetailsViewModel(Model.Request request)
        {
            // Инициализация сервисов
            _requestService = new RequestService();
            _messageService = new MessageService();

            Request = _requestService.GetRequestById(request.Id) ?? request;

            PageTitle = BuildPageTitle(Request);
            OnPropertyChanged(nameof(PageTitle));

            LoadData();

            GoBackCommand = new RelayCommand(_ => GoBack?.Invoke());
            ClientDetailsCommand = new RelayCommand(_ => NavigateToClientDetails?.Invoke(Request.Client));
        
            AddCommentCommand = new RelayCommand(
                _ => CreateOrEditComment(SelectedRequestComment), 
                _ => !string.IsNullOrWhiteSpace(CommentText) && CheckCommentPrivileges(SelectedRequestComment));

            EditCommentCommand = new RelayCommand(
                _ => EditCommentManually(SelectedRequestComment), 
                _ => _selectedRequestComment != null && CheckCommentPrivileges(SelectedRequestComment));

            RemoveCommentCommand = new RelayCommand(
                _ => RemoveComment(SelectedRequestComment), 
                _ => (_selectedRequestComment != null && CheckCommentPrivileges(SelectedRequestComment)) 
                    || (CurrentSession.IsAdmin && _selectedRequestComment != null));

            SetRequestStatusCommand = new RelayCommand(
                _ => UpdateRequestStatus(Request.Id, SelectedStatusId), 
                _ => SelectedStatusId.HasValue);

            AssignedToRequestCommand = new RelayCommand(
               _ => AssignedCurrentUser(Request.Id), 
               _ => Request.Employee == null);

            EditCommand = new RelayCommand(_ => NavigateToAddEditPage?.Invoke(Request));

        }

        // === Вспомогательные методы === 

        // Назначить текущего пользователяя на заявку
        public void AssignedCurrentUser(int requestId)
        {
            var currentEmployee = CurrentSession.CurrentUser?.Employee;
            if (currentEmployee == null) return;

            try
            {
                var editedRequest = new Model.Request
                {
                    Id = requestId,
                    EmployeeId = currentEmployee.Id,
                };

                _requestService.EditRequest(editedRequest);
                RefreshRequest();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
            }
        }

        // Обновить статус заявки
        public void UpdateRequestStatus(int requestId, int? statusId)
        {
            if (statusId == null) return;

            try
            {
                var editedStatus = new Model.Request
                {
                    Id = requestId,
                    StatusId = statusId,
                };

                _requestService.EditRequest(editedStatus);
                RefreshRequest();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
            }
        }

        // Построение заголовки страницы 
        private static string BuildPageTitle(Model.Request request)
        {
            if (request.Id == 0)
                return "Новая заявка";

            var created = request.CreatedAt;
            var datePart = created.HasValue
                ? created.Value.ToString("dd.MM.yyyy HH:mm")
                : "—";

            return $"Информация о заявке №{request.Id:D4} от {datePart}";
        }

        // === Управление комментариями ===

        // Создать или отредактировать комментарий
        public void CreateOrEditComment(RequestComment? requestComment)
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(CommentText))
                errors.AppendLine("Комментарий не может быть пустым.");
            else if (CommentText?.Length > 255)
                errors.AppendLine("Комментарий не может быть больше 255 символов!");
            
            if (errors.Length > 0)
            {
                _messageService.ShowError(errors.ToString());
                return;
            }

            try
            {
                if (requestComment == null)
                {
                    var newComment = new RequestComment
                    {
                        CreatedAt = DateTime.Now,
                        CreatedBy = CurrentSession.CurrentUser?.Employee?.Id ?? 0,
                        RequestId = Request?.Id ?? 0,
                        Comment = CommentText?.Trim() ?? string.Empty,
                    };

                    _requestService.AddComment(newComment);
                    LoadCommentsData();
                }
                else
                {
                    var existingComment = _requestService.GetCommentById(requestComment.Id);
                    if (existingComment != null)
                    {
                        var updateComment = new RequestComment
                        {
                            Id = existingComment.Id,
                            CreatedAt = existingComment.CreatedAt,
                            CreatedBy = existingComment.CreatedBy,
                            RequestId = existingComment.RequestId,
                            Comment = CommentText?.Trim() ?? existingComment.Comment,
                        };

                        _requestService.EditComment(updateComment);
                        LoadCommentsData();
                    }
                }

                CommentText = string.Empty;
                SelectedRequestComment = null;
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
            }
        }

        // Удаление комментария
        private void RemoveComment(RequestComment? comment)
        {
            if (comment == null) return;
            
            try
            {
                if (RequestComments.Contains(comment))
                {
                    _requestService.RemoveComment(comment);
                }    

                LoadCommentsData();
            }
            catch (Exception ex)
            {
                _messageService.ShowError(ex.Message);
            }

            CommentText = string.Empty;
            SelectedRequestComment = null;
        }
        
        // Проверка принадлежит ли комментарий текущему пользователю
        private bool CheckCommentPrivileges(RequestComment? requestComment)
        {
            if (requestComment == null) return true;

            return CurrentSession.CurrentUser?.Employee?.Id == requestComment?.CreatedByNavigation.Id;
        }
        
        // Установка текста для поля ввода, на основе текста выбранного комментария 
        public void EditCommentManually(RequestComment? requestComment)
        {
            if (requestComment != null)
            {
                CommentText = requestComment?.Comment ?? string.Empty;
            }
        }

        // === Методы загрузки данных ===

        // Обновление заявки
        public void RefreshRequest()
        {
            if (Request == null) return;

            Request = _requestService.GetRequestById(Request.Id) ?? Request;

            PageTitle = BuildPageTitle(Request);
            OnPropertyChanged(nameof(PageTitle));

            AssignedToRequestCommand.RaiseCanExecuteChanged();
            SetRequestStatusCommand.RaiseCanExecuteChanged();
        }

        // Загрузка комменатриев
        public void LoadCommentsData()
        {
            RequestComments.Clear();
            var requestComments = _requestService.GetRequestComments(Request?.Id ?? 0);
            foreach (var comment in requestComments)
            {
                RequestComments.Add(comment);
            }
        }

        // Загрузка статусов
        public void LoadStatusesData()
        {
            RequestStatuses.Clear();
            var requestStatuses = _requestService.GetAllStatuses();
            foreach (var status in requestStatuses)
            {
                RequestStatuses.Add(status);
            }
        }

        // Общий метод загрузки данных 
        public void LoadData()
        {
            LoadCommentsData();
            LoadStatusesData();
        }

        // Метод для обновления страницы, при переходе
        public void Refresh()
        {
            RefreshRequest();
            LoadData();
        }
    }
}
