# ExpensesTelegramBot

A Telegram bot for tracking personal expenses. You send it short text messages like `1500 lunch`, and it saves them, shows recent entries, sums up a month and exports it to CSV for Google Sheets.

## Adding expenses

Send a message in the format:

```
[<date>] <amount>[k] [description]
```

- `date` is optional: `yyyy-MM-dd`, `yyyy.MM.dd` or `yyyy/MM/dd`. Without it the expense goes to today, in your time zone.
- `amount` is a whole number. A `k` or `K` suffix multiplies it by 1000. Currencies aren't supported.
- `description` is optional free text.

Each line of a message is a separate expense, so you can send several at once:

```
1500 lunch
20k rent
2022-08-04 300 taxi
```

## Commands

| Command | What it does |
|---|---|
| `/help` | Shows the full help |
| `/get` | The last 5 expenses |
| `/getall` | All expenses for the current month, by date |
| `/sum [[<year>] <month>]` | The total for a month: `/sum`, `/sum 8`, `/sum 2022 8` |
| `/export` | A CSV file for the current month with the total for each day, ready to import into Google Sheets |
| `/settimezone <offset>` | Sets your time zone as a UTC offset: `3`, `+3`, `-08`, `+5:30` |

## Storage

Expenses are kept in CSV files on the bot's machine, one per chat: `data/<chat id>/expenses.csv`. The time zone setting sits next to them in `data/<chat id>/userSettings.cfg`.

## Running

Requirements: .NET Core 3.1 SDK. The bot uses [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) 18.

1. Create a bot with [@BotFather](https://t.me/BotFather) and get its token.
2. Put the token into the `token` constant in `Program.cs`.
3. Run:

   ```sh
   dotnet run
   ```

The `Dockerfile` builds an image for ARM64 (for example, a Raspberry Pi). Mount `/app/data` as a volume so expenses survive container restarts.

## License

[MIT](LICENSE)
