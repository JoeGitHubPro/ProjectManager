using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

public class BaseViewModel : INotifyPropertyChanged//, IDataErrorInfo
{
    public event PropertyChangedEventHandler PropertyChanged;
    private readonly Dictionary<string, string> _errors = new();
    protected bool SetProperty<T>(ref T field, T value, bool validate = false, [CallerMemberName] string propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);

        if (validate) ValidateProperty(propertyName, value);

        return true;
    }
    private void ValidateProperty(string propertyName, object value)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this) { MemberName = propertyName };

        if (!Validator.TryValidateProperty(value, validationContext, validationResults))
        {
            _errors[propertyName] = string.Join("\n", validationResults.Select(vr => vr.ErrorMessage));
        }
        else
        {
            _errors.Remove(propertyName);
        }

        OnPropertyChanged(propertyName);
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}