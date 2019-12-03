namespace Parallel.Location
{
    public interface ILocationCalculatorRouter<in TKey>
    {
        ILocationCalculator GetCalculator(TKey key);
    }
}