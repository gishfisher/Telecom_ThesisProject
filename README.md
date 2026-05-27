# Telecom ThesisProject

Дипломный проект - десктопное WPF-приложение для управления телекоммуникационной инфраструктурой. Система позволяет вести учёт клиентов, сотрудников, тарифов, услуг, заявок, сетевых устройств и точек монтажа. Взаимодействие с данными реализовано через Entity Framework Core (Database-First) и SQL Server.

## Начало работы

Инструкции ниже помогут развернуть копию проекта на локальном компьютере для разработки и тестирования.

### Необходимые условия

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8) - для сборки из исходников (не нужен если используете готовый `.exe`)
- [SQL Server](https://www.microsoft.com/ru-ru/sql-server/sql-server-downloads) - любая редакция (Express подойдёт)
- [SSMS](https://learn.microsoft.com/ru-ru/sql/ssms/download-sql-server-management-studio-ssms) - для выполнения SQL-скриптов

```
SQL Server 2019 или новее
Windows 10 / 11 x64
```

### Установка

**Шаг 1.** Клонируйте репозиторий или скачайте ZIP из раздела [Releases](../../releases)

```bash
git clone https://github.com/gishfisher/Telecom_ThesisProject.git
```

**Шаг 2.** Откройте SSMS, подключитесь к серверу и выполните скрипты из папки `database\` по порядку

```
database\01_create_database.sql   - создаёт базу данных и таблицы
database\02_seed_data.sql         - заполняет справочники и создаёт учётную запись администратора
```

**Шаг 3.** Откройте файл `appsettings.json` рядом с `.exe` и укажите ваш сервер

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=ИМЯ_СЕРВЕРА;Database=TelecomDB;Integrated Security=True;TrustServerCertificate=True;"
  }
}
```

**Шаг 4.** Запустите приложение

```
Telecom_ThesisProject.exe
```

Войдите с учётными данными администратора. Стандартные данные для входа admin admin.

## Сборка из исходников

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

Готовый `.exe` появится в папке `publish\`.

## Стек технологий

- **WPF** (.NET 8) - пользовательский интерфейс
- **Entity Framework Core** - доступ к данным (Database-First)
- **SQL Server** - база данных
- **MVVM** - архитектурный паттерн

## Авторы

* **Шумаков Вадим Евгеньевич** - *Дипломный проект* - [GitHub]((https://github.com/gishfisher))
