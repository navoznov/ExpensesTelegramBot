using ExpensesTelegramBot.Repositories;

namespace ExpensesTelegramBot.Telegram.Commands.SetTimeZone
{
    public class SetTimeZoneCommand : Command
    {
        private readonly long _userId;
        private readonly IUserSettingsRepository _userSettingsRepository;

        public SetTimeZoneCommand(SetTimeZoneCommandInput input, long userId,
            IUserSettingsRepository userSettingsRepository)
            : base(input)
        {
            _userId = userId;
            _userSettingsRepository = userSettingsRepository;
        }

        public override CommandResult Execute()
        {
            var setTimeZoneCommandInput = (SetTimeZoneCommandInput)Input;
            var userSettingsInfo = _userSettingsRepository.GetUserSettingsInfo(_userId);
            userSettingsInfo.TimeZoneInfo = setTimeZoneCommandInput.TimeZoneInfo;
            _userSettingsRepository.Save(userSettingsInfo);
            return new CommandTextResult($"Time zone \"{userSettingsInfo.TimeZoneInfo.Id}\" saved");
        }
    }
}