using Stylet;

namespace KorabliChsMod.ViewModels
{
    public class RootViewModel : PropertyChangedBase
    {
        private string _title = "HandyControl Application";
        public string Title
        {
            get { return _title; }
            set { SetAndNotify(ref _title, value); }
        }
    }
}
