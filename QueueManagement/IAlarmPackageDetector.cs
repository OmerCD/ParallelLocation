namespace QueueManagement
{
    public interface IAlarmPackageDetector
    {
        /// <summary>
        /// Alarm taşıma ihtimali olan paketin alarm içerip içermediğini gösterir.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        bool IsAlarmPackage(byte[] package);
        
        /// <summary>
        /// Paketin alarm kontrolüne girip giremeyeceğini verir. Örneğin; 210 lu paketin alarm taşıma ihtimali yok.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        bool IsPackageSatisfies(byte[] package);
    }
}