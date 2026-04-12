using System.Windows;
using Telecom_ThesisProject.Utilities.Interfaces;

namespace Telecom_ThesisProject.Services
{
    public class MessageService : IMessageService
    {
        public void Show(string message)
        {
            MessageBox.Show(message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool Confirm(string message)
        {
            var result = MessageBox.Show(message, "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }
    }
}
