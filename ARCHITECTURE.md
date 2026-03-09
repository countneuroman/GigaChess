# Архитектура GigaChess

## Controller → Service
Контроллеры тонкие: вызывают методы сервиса и маппят результат в HTTP-статусы (Ok / BadRequest / NotFound). Вся бизнес-логика живёт в `Services/`.

## Result\<T>
Сервисы возвращают `Result<T>` (`Common/Result.cs`) для передачи success/fail/not-found без привязки к ASP.NET типам.

## DI
Сервисы регистрируются в `Program.cs` (Singleton или Scoped).

## Представление доски
- `string[,]` 8×8 массив
- Фигуры: `"wP"`, `"bK"`, `null` = пустая клетка
- Row 0 = rank 8 (чёрные), Row 7 = rank 1 (белые)
