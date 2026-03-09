# AGENTS.md — GigaChess

This project uses **bd** (beads) for issue tracking. Run `bd onboard` to get started.

Структура проекта, стек и команды сборки — см. [README.md](README.md).
Архитектурные паттерны и детали реализации — см. [ARCHITECTURE.md](ARCHITECTURE.md).

## Issue tracker (beads)

```bash
bd ready              # Найти доступную работу
bd show <id>          # Детали задачи
bd update <id> --status in_progress  # Взять задачу
bd close <id>         # Закрыть задачу
bd sync               # Синхронизация с git
bd list               # Список задач
bd create -t "Title" -d "..."  # Создать задачу
bd children <id>      # Подзадачи эпика
bd search "текст"     # Поиск по задачам
```

**Правила:**
- Закрывай задачи (`bd close`) по завершении работы над ними.
- После закрытия задачи — делай коммит в git (без push в remote).

## Starting a Session

**При начале новой сессии** ВСЕГДА выполняй следующее, прежде чем приступать к работе:

1. **Проверь последние коммиты** — `git log --oneline -10`
2. **Проверь состояние задач** — `bd list --all`
3. **Кратко сообщи пользователю** — что было сделано в прошлый раз и какие задачи следующие

## Landing the Plane (Session Completion)

**When ending a work session**, you MUST complete ALL steps below. Work is NOT complete until `git push` succeeds.

**MANDATORY WORKFLOW:**

1. **File issues for remaining work** - Create issues for anything that needs follow-up
2. **Run quality gates** (if code changed) - Tests, linters, builds
3. **Update issue status** - Close finished work, update in-progress items
4. **PUSH TO REMOTE** - This is MANDATORY:
   ```bash
   git pull --rebase
   bd sync
   git push
   git status  # MUST show "up to date with origin"
   ```
5. **Clean up** - Clear stashes, prune remote branches
6. **Verify** - All changes committed AND pushed
7. **Hand off** - Provide context for next session

**CRITICAL RULES:**
- Work is NOT complete until `git push` succeeds
- NEVER stop before pushing - that leaves work stranded locally
- NEVER say "ready to push when you are" - YOU must push
- If push fails, resolve and retry until it succeeds

## Конвенции для агентов

- **Не использовать `cd`** — рабочая директория уже правильная
- **Не добавлять `2>&1`** к командам без явной необходимости
- **Предпочитать тесты** вместо ручного запуска сервера для верификации
- **Не использовать `-C` флаг** в git-командах — уже в нужной директории
