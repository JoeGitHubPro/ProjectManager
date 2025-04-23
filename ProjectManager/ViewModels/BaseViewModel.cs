using System.ComponentModel;
using System.Runtime.CompilerServices;

public class BaseViewModel : INotifyPropertyChanged, IDataErrorInfo
{
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // IDataErrorInfo implementation
    public virtual string Error => string.Empty;

    public virtual string this[string columnName] => string.Empty;

    // Add this property
    public Dictionary<string, string> ErrorCollection
    {
        get
        {
            var errors = new Dictionary<string, string>();
            var properties = this.GetType().GetProperties();

            foreach (var property in properties)
            {
                var error = this[property.Name];
                if (!string.IsNullOrEmpty(error))
                {
                    errors.Add(property.Name, error);
                }
            }
            return errors;
        }
    }
}