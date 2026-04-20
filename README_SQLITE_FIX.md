# Решение проблемы "Library e_sqlite3 not found"

## Проблема
При запуске приложения возникает ошибка:
```
System.TypeInitializationException: Инициализатор типа "Microsoft.Data.Sqlite.SqliteConnection" выдал исключение.
Внутреннее исключение: Exception: Library e_sqlite3 not found
```

## Причина
Отсутствуют нативные библиотеки SQLite (e_sqlite3.dll) в выходной папке проекта.

## Решение

### Способ 1: Переустановка NuGet пакета (Рекомендуется)
В Visual Studio Package Manager Console выполните:
```
Update-Package SQLitePCLRaw.bundle_e_sqlite3 -Reinstall
```
Или через UI:
1. Правый клик на проекте → Manage NuGet Packages
2. Удалите SQLitePCLRaw.bundle_e_sqlite3
3. Установите его заново
4. Пересоберите проект

### Способ 2: Ручное копирование DLL
1. Найдите файл e_sqlite3.dll в папке NuGet:
   `%USERPROFILE%\.nuget\packages\sqlitepclraw.lib.e_sqlite3\2.1.2\runtimes\win-x64\native\e_sqlite3.dll`

2. Скопируйте его в папку вывода проекта:
   - `bin\Debug\e_sqlite3.dll`
   - ИЛИ `bin\Debug\runtimes\win-x64\native\e_sqlite3.dll`

### Способ 3: Изменения в .csproj файле
Файл WindowsFormsApp12.csproj уже обновлён для автоматического копирования библиотек при сборке.

## Проверка
После применения решения убедитесь, что в папке bin\Debug\ присутствуют:
- e_sqlite3.dll (в корне папки Debug)
- runtimes\win-x64\native\e_sqlite3.dll
- runtimes\win-x86\native\e_sqlite3.dll

## Структура файлов после исправления
```
bin/Debug/
├── WindowsFormsApp12.exe
├── Microsoft.Data.Sqlite.dll
├── ... (другие DLL)
├── e_sqlite3.dll (опционально, как fallback)
└── runtimes/
    ├── win-x64/
    │   └── native/
    │       └── e_sqlite3.dll
    └── win-x86/
        └── native/
            └── e_sqlite3.dll
```
