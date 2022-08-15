**! Expense registration**

Send a message with text in the format:
`[<date>] <money>[<multiplier>] [description]`

Fileds:
`date` - the date of the expense. Optional field. Default value is the current month/year.

Supported formats:
1. `yyyy-MM-dd`. For example: `2022-08.04`
2. `yyyy.MM.dd`. For example: `2022.08.04`

`money` - the amount of the expense. Currency isn't supported. The field is required.

`multiplier` - a letter that tells the bot to multiply money amount by constant value.
The accepted values of the multiplier are: 
1. `K` multiplies by 1000
2. `k` multiplies by 1000

`description` - text description of the expense. Shopping, lunch, car cleaning, etc. The description field is optional.

Examples:
`1000 lunch` - registers an expense for today. Money value is 1000. Description is `lunch`
`2022-08-04 20K` - registers an expense for 4 august 2022. Money is 20000. Description is empty.

-----
**! Getting last expenses**
Description: sends you last registered expenses.
Usage: `/get [<count>]`
Fields:
`count` - positive integer number that represents expenses count in the output. is . Field is optional. Default value is `5`.
Examples:
`/get` - returns last 5 entered expenses
`/get 10` - returns last 10 entered expenses

-----
**! Getting all current month expenses**
Description: returns all expenses for the current month ordered by date.
Usage: `/getall`

-----
**! Get sum of the month expenses**
Usage: `/sum [[<year>] <month>]`
Description: Sums up month expenses (summed by day) and outputs the result.
Fields:
`year` - the year of expenses for summing. This field is optional. If not specified, the bot will use the current year. If the year field is specified, you must specify the month field.
`month` - the month of expenses for summing. This field is optional. If not specified, the bot will use the current month.
Examples:
`/sum` sums up expenses of the current month
`/sum 8` sums up expenses of august (8th month) of the current year
`/sum 2022 08` sums up expenses of august 2022

-----
**! Export expenses to CSV**
Usage: `/export [[<year>] <month>]`
Description: Exports to CSV-file days of the month with expenses sum for each day. It could be useful to import csv-file to Google Sheets. 
Fields:
`year` - the year of expenses for export. This field is optional. If not specified, the bot will use the current year. If the year field is specified, you must specify the month field.
`month` - the month of expenses for export. This field is optional. If not specified, the bot will use the current month. 
Examples:
`/export` exports aggregated expenses of the current month
`/export 8` exports aggregated expenses of august (8th month) of the current year
`/export 2022 08` exports aggregated expenses of august 2022
