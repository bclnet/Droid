using System.Text;

namespace Droid.Core
{
    public interface IEditField
    {
        void AutoComplete();
        void Clear();
        void ClearAutoComplete();
        StringBuilder Buffer { get; }
        int Cursor { get; set; }
    }
}