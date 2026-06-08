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
        "DefaultConnection": "Server=ИМЯ_СЕРВЕРА;Database=TelecomDataBase;Integrated Security=True;TrustServerCertificate=True;"
    }
}
```

**Шаг 4.** Запустите приложение

```
Telecom_ThesisProject.exe
```

Войдите с учётными данными администратора. Стандартные данные для входа 
- Логин: `admin`
- Пароль: `admin`.

## Сборка из исходников

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o ./publish
```

Готовый `.exe` появится в папке `publish\`.

## Настройка эмулятора EVE-NG

### 1. Установка гипервизора
* **VMware Workstation Pro** - [Скачать с официального сайта](https://www.vmware.com/products/desktop-hypervisor/workstation-and-fusion)
  > **Примечание:** При скачивании и установке могут возникнуть трудности из-за региональных ограничений со стороны компании Broadcom.

### 2. Развертывание платформы
* **EVE-NG Community** - [Страница загрузки](https://www.eve-ng.net/index.php/download/)
* **Инструкции** - [Официальное руководство по установке (Community Cookbook)](https://www.eve-ng.net/index.php/documentation/community-cookbook/)

### 3. Инструменты для работы
Для загрузки файлов и управления виртуальной машиной вам понадобятся:

* **SFTP-клиент (на выбор):**
  * [FileZilla](https://filezilla.ru/get/) - популярный кроссплатформенный клиент.
  * [WinSCP](https://winscp.net/eng/download.php) - удобный клиент для Windows.
* **SSH-клиент:**
  * [PuTTY](https://putty.org.ru/) - легкий терминал для подключения к консоли.

## Инструкция по установке Cisco IOL и импорту топологии в EVE-NG

### 1. Загрузка образов и файлов лицензии
1. Откройте SFTP-клиент (**FileZilla** или **WinSCP**).
2. Подключитесь к виртуальной машине EVE-NG:
   * **Имя пользователя:** `root`
   * **Пароль:** `eve`
3. Перейдите в директорию:
   ```bash
   /opt/unetlab/addons/iol/bin/
   ```
4. Скопируйте в эту папку:
   * Образы IOL (файлы `*.bin`, например, *x86_64_crb_linux_l2-adventerprisek9-ms*)
   * Скрипт активации `Key.py`

---

### 2. Настройка прав и активация лицензии
1. Подключитесь к ВМ EVE-NG через SSH-клиент (**Putty**), используя те же учетные данные (`root` / `eve`).
2. Перейдите в рабочую директорию:
   ```bash
   cd /opt/unetlab/addons/iol/bin/
   ```
3. Сделайте файлы исполняемыми и исправьте права доступа (это можно сделать через свойства файлов в FileZilla/WinSCP или командами в Putty):
   ```bash
   chmod +x *
   /opt/unetlab/wrappers/unl_wrapper -a fixpermissions
   ```
4. Запустите скрипт генерации лицензии:
   ```bash
   python2 Key.py
   ```
5. **Выделите и скопируйте** сгенерированный текст лицензии из окна консоли.
6. Создайте и откройте файл лицензии для редактирования:
   ```bash
   nano iourc
   ```
7. Вставьте скопированный текст (нажмите **ПРАВУЮ КНОПКУ МЫШИ** в окне Putty).
8. Сохраните изменения: нажмите `Ctrl + X`, затем `Y` и `Enter`.

---

### 3. Импорт топологии в веб-интерфейсе
> **Важно:** Конфигурация устройств при импорте `.zip` архива не восстанавливается. Убедитесь, что используемые вами образы Cisco IOL соответствуют OID-идентификаторам импортированного оборудования.

1. Откройте веб-интерфейс EVE-NG в браузере.
2. Пройдите аутентификацию:
   * **Имя пользователя:** `admin`
   * **Пароль:** `eve`
3. На главной странице нажмите кнопку **Import**.
4. Выберите нужный архив: `_Exports_unetlab_export-20260528-220406.zip` (по пути `publish/eve-ng/`).
5. Нажмите кнопку **Upload** для завершения импорта.


## Стек технологий

- **WPF** (.NET 8) - пользовательский интерфейс
- **Entity Framework Core** - доступ к данным (Database-First)
- **SQL Server** - база данных
- **MVVM** - архитектурный паттерн
- **EVE-NG** - внешний источник данных

## Авторы

* **Шумаков Вадим Евгеньевич** - *Дипломный проект* - [GitHub](https://github.com/gishfisher)
