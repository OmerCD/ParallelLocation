using System.Collections.Generic;

namespace QueueManagement
{
    public interface IAlarmPackageRouter
    {
        bool Route(byte[] package, out IAlarmPackageDetector alarmPackageDetector);
    }

    public class AlarmPackageRouter:IAlarmPackageRouter
    {
        private readonly IEnumerable<IAlarmPackageDetector> _alarmPackageDetectors;

        public AlarmPackageRouter(IEnumerable<IAlarmPackageDetector> alarmPackageDetectors)
        {
            _alarmPackageDetectors = alarmPackageDetectors;
        }
        
        public bool Route(byte[] package, out IAlarmPackageDetector alarmPackageDetector)
        {
            foreach (var item in _alarmPackageDetectors)
            {
                if (item.IsPackageSatisfies(package) && item.IsAlarmPackage(package))
                {
                    alarmPackageDetector = item;
                    return true;
                }
            }

            alarmPackageDetector = null;
            return false;
        }
    }
}