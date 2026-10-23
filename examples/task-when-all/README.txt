dotnet-audit-studio — Task.WhenAll: три учебных сценария

Собственный синтетический пример, без клиентского кода, сети и нагрузки.
Сохранённый запуск: .NET SDK 10.0.301, runtime .NET 10.0.9.
В архиве исходники, отдельные буквальные ожидания, сохранённые наблюдения
и независимая сверка. 485 проверок относятся к трём сценариям.

1. Две ошибки и ещё незавершённая задача: объединённая задача ждёт последнего
завершения, затем имеет Faulted и обе ошибки в Exception.InnerExceptions.
2. Ошибка вместе с отменой: итог Faulted; отмена не стала второй fault.
3. Отмена без ошибок: итог Canceled, Exception=null; await выбрасывает
исключение, совместимое с OperationCanceledException.

Порядок ошибок и выбор конкретного исключения для await не задаются
как бизнес-контракт. Вложенные AggregateException и Flatten не проверялись.
Сетевые вызовы, retries, production logging и восстановление не проверялись.

Проверка сохранённых наблюдений без нового запуска C#:
python verify_observed.py --observed observed.json --expected expected.json --receipt verification-current.json

Воспроизведение при уже установленном SDK 10.0.301:
dotnet restore TaskWhenAllProof.csproj --configfile NuGet.Config --ignore-failed-sources --nologo
dotnet build TaskWhenAllProof.csproj --no-restore --configuration Release --nologo -p:UseSharedCompilation=false
dotnet bin/Release/net10.0/TaskWhenAllProof.dll > observed-current.json
python verify_observed.py --observed observed-current.json --expected expected.json --receipt verification-current.json

Package dependencies отсутствуют, NuGet.Config не задаёт сетевых источников.
global.json фиксирует SDK; другая версия не выдаётся за проверенную.
Пути и имена в manifest.json относительные; хеши относятся к байтам файлов.

Первичные источники:
https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.whenall?view=net-10.0
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/
https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.exception?view=net-10.0
