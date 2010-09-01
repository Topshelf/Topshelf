using System;

namespace Topshelf.WindowsServiceCode
{
  public class ServiceRecoveryOptions : IServiceRecoveryOptions
  {
    public ServiceRecoveryOptions()
    {
      FirstFailureAction = ServiceRecoveryAction.TakeNoAction;
      SecondFailureAction = ServiceRecoveryAction.TakeNoAction;
      SubsequentFailureActions = ServiceRecoveryAction.TakeNoAction;
      DaysToResetFailAcount = 0;
      MinutesToRestartService = 1;
    }

    public ServiceRecoveryAction FirstFailureAction { get; set; }
    public ServiceRecoveryAction SecondFailureAction { get; set; }
    public ServiceRecoveryAction SubsequentFailureActions { get; set; }
    public int DaysToResetFailAcount { get; set; }
    public int MinutesToRestartService { get; set; }
    public string RebootMessage { get; set; }
    public string CommandToLaunchOnFailure { get; set; }

    public void Validate()
    {
      ThrowHelper.ThrowInvalidOperationExceptionIf(
          s =>
          !string.IsNullOrEmpty(s.RebootMessage) &&
          !s.RecoveryActionIsDefined(ServiceRecoveryAction.RestartTheComputer),
          this,
          "Setting '{0}' is not valid when there is no '{1}' failure action/s defined.",
          "RebootMessage", ServiceRecoveryAction.RestartTheComputer);

      ThrowHelper.ThrowInvalidOperationExceptionIf(
          s =>
          !string.IsNullOrEmpty(s.CommandToLaunchOnFailure) &&
          !s.RecoveryActionIsDefined(ServiceRecoveryAction.RunAProgram),
          this,
          "Setting '{0}' is not valid when there is no '{1}' failure action/s defined.",
          "CommandToLaunchOnFailure", ServiceRecoveryAction.RunAProgram);

      ThrowHelper.ThrowInvalidOperationExceptionIf(
          s =>
          s.MinutesToRestartService > 1 &&
          !s.RecoveryActionIsDefined(ServiceRecoveryAction.RestartTheService),
          this,
          "Setting '{0}' is not valid when there is no '{1}' failure action/s defined.",
          "MinutesToRestartService", ServiceRecoveryAction.RestartTheService);
    }

    private bool RecoveryActionIsDefined(ServiceRecoveryAction action)
    {
      return FirstFailureAction == action ||
             SecondFailureAction == action ||
             SubsequentFailureActions == action;
    }

    public override bool Equals(object other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      if (other.GetType() != typeof(ServiceRecoveryOptions)) return false;
      return Equals((ServiceRecoveryOptions)other);
    }

    public bool Equals(ServiceRecoveryOptions other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return Equals(other.FirstFailureAction, FirstFailureAction) && Equals(other.SecondFailureAction, SecondFailureAction) && Equals(other.SubsequentFailureActions, SubsequentFailureActions) && other.DaysToResetFailAcount == DaysToResetFailAcount && other.MinutesToRestartService == MinutesToRestartService && Equals(other.RebootMessage, RebootMessage) && Equals(other.CommandToLaunchOnFailure, CommandToLaunchOnFailure);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        int result = FirstFailureAction.GetHashCode();
        result = (result * 397) ^ SecondFailureAction.GetHashCode();
        result = (result * 397) ^ SubsequentFailureActions.GetHashCode();
        result = (result * 397) ^ DaysToResetFailAcount;
        result = (result * 397) ^ MinutesToRestartService;
        result = (result * 397) ^ (RebootMessage != null ? RebootMessage.GetHashCode() : 0);
        result = (result * 397) ^ (CommandToLaunchOnFailure != null ? CommandToLaunchOnFailure.GetHashCode() : 0);
        return result;
      }
    }
  }


  static class ThrowHelper
  {
    public static void ThrowArgumentNullIfNull(object o, string paramName)
    {
      if (o == null)
      {
        throw new ArgumentNullException(paramName);
      }
    }

    public static void ThrowArgumentOutOfRangeIf<T>(Predicate<T> predicate,
                                                    T value,
                                                    string paramName,
                                                    string message)
    {
      if (predicate(value))
      {
        throw new ArgumentOutOfRangeException(paramName, message);
      }
    }

    public static void ThrowInvalidOperationExceptionIf<T>(Predicate<T> predicate,
                                                           T value,
                                                           string message)
    {
      if (predicate(value))
      {
        throw new InvalidOperationException(message);
      }
    }

    public static void ThrowInvalidOperationExceptionIf<T>(Predicate<T> predicate,
                                                           T value,
                                                           string message,
                                                           params object[] args)
    {
      if (predicate(value))
      {
        throw new InvalidOperationException(string.Format(message, args));
      }
    }

    public static void ThrowArgumentOutOfRangeIf<T>(Predicate<T> predicate,
                                                    T value,
                                                    string paramName)
    {
      if (predicate(value))
      {
        throw new ArgumentOutOfRangeException(paramName);
      }
    }

    public static void ThrowArgumentOutOfRangeIfEmpty(string s,
                                                      string paramName)
    {
      ThrowArgumentOutOfRangeIf(str => string.IsNullOrEmpty(str), s, paramName);
    }

    public static void ThrowArgumentOutOfRangeIfEmpty(char c,
                                                      string paramName)
    {
      ThrowArgumentOutOfRangeIf(Char.IsWhiteSpace, c, paramName);
    }

    public static void ThrowArgumentOutOfRangeIfZero(int i,
                                                     string paramName)
    {
      ThrowArgumentOutOfRangeIf(num => num == 0, i, paramName,
          string.Format("Argument '{0}' must not be equal to '0'(zero) ", paramName));
    }
  }

}
