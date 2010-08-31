namespace Topshelf.WindowsServiceCode
{
  public interface IServiceRecoveryOptions
  {
    ServiceRecoveryAction FirstFailureAction { get; set; }
    ServiceRecoveryAction SecondFailureAction { get; set; }
    ServiceRecoveryAction SubsequentFailureActions { get; set; }
    int DaysToResetFailAcount { get; set; }
    int MinutesToRestartService { get; set; }
    string RebootMessage { get; set; }
    string CommandToLaunchOnFailure { get; set; }
    void Validate();
    bool Equals(object other);
    bool Equals(ServiceRecoveryOptions other);
    int GetHashCode();
  }
}