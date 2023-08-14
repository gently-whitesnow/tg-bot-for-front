using Sorcer.Models.Event;
using Sorcer.Models.RwLock;
using Sorcer.Models.User;

namespace Sorcer.DataAccess.Repositories;

public class UserStateRepository
{
    private readonly List<TempUserData> _userStates = new();
    private readonly RWLock _lock = new RWLock();

    public TempUserData GetTempUserData(long userId)
    {
        using (_lock.ReadLock())
        {
            return _userStates.FirstOrDefault(u => u.UserId == userId);
        }
    }
    
    public void ClearTempUserData(long userId)
    {
        using (_lock.WriteLock())
        {
            var tempUserData = _userStates.FirstOrDefault(u => u.UserId == userId);
            if(tempUserData!=null)
                _userStates.Remove(tempUserData);
        }
    }

    public void SetTempUserData(long userId, Action<TempUserData> action)
    {
        using (var _upgradeableLock = _lock.ReadUpgradeableLock())
        {
            var userState = _userStates.FirstOrDefault(u => u.UserId == userId);
            _upgradeableLock.EnterWriteLock();
            try
            {
                if (userState == null)
                {
                    userState = new TempUserData
                    {
                        UserId = userId,
                        EventDto = new EventDto()
                    };
                    action(userState);
                    _userStates.Add(userState);
                }
                else
                {
                    action(userState);
                }
            }
            finally
            {
                _upgradeableLock.ExitWriteLock();
            }
        }
    }
}