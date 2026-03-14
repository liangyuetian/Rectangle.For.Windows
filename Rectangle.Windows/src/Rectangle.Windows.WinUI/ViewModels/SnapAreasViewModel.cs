using System.Threading.Tasks;
using Rectangle.Windows.WinUI.Services;

namespace Rectangle.Windows.WinUI.ViewModels
{
    public class SnapAreasViewModel : ObservableObject
    {
        private readonly ConfigService _configService;
        private bool _dragToSnap;
        private bool _restoreSizeOnSnapEnd;
        private bool _snapAnimation;
        private bool _hapticFeedback;
        private int _gapSize;

        public bool DragToSnap
        {
            get => _dragToSnap;
            set { if (SetProperty(ref _dragToSnap, value)) SaveSettings(); }
        }

        public bool RestoreSizeOnSnapEnd
        {
            get => _restoreSizeOnSnapEnd;
            set { if (SetProperty(ref _restoreSizeOnSnapEnd, value)) SaveSettings(); }
        }

        public bool SnapAnimation
        {
            get => _snapAnimation;
            set { if (SetProperty(ref _snapAnimation, value)) SaveSettings(); }
        }

        public bool HapticFeedback
        {
            get => _hapticFeedback;
            set { if (SetProperty(ref _hapticFeedback, value)) SaveSettings(); }
        }

        public int GapSize
        {
            get => _gapSize;
            set { if (SetProperty(ref _gapSize, value)) SaveSettings(); }
        }

        public SnapAreasViewModel()
        {
            _configService = new ConfigService();
        }

        public async Task LoadSettingsAsync()
        {
            await Task.Run(() =>
            {
                var config = _configService.Load();
                _dragToSnap = config.SnapAreas?.DragToSnap ?? true;
                _restoreSizeOnSnapEnd = config.SnapAreas?.RestoreSizeOnSnapEnd ?? true;
                _snapAnimation = config.SnapAreas?.SnapAnimation ?? false;
                _hapticFeedback = config.SnapAreas?.HapticFeedback ?? false;
                _gapSize = config.GapSize;
                OnPropertyChanged(nameof(DragToSnap));
                OnPropertyChanged(nameof(RestoreSizeOnSnapEnd));
                OnPropertyChanged(nameof(SnapAnimation));
                OnPropertyChanged(nameof(HapticFeedback));
                OnPropertyChanged(nameof(GapSize));
            });
        }

        private void SaveSettings()
        {
            var config = _configService.Load();
            config.SnapAreas ??= new SnapAreaConfig();
            config.SnapAreas.DragToSnap = _dragToSnap;
            config.SnapAreas.RestoreSizeOnSnapEnd = _restoreSizeOnSnapEnd;
            config.SnapAreas.SnapAnimation = _snapAnimation;
            config.SnapAreas.HapticFeedback = _hapticFeedback;
            config.GapSize = _gapSize;
            _configService.Save(config);
        }
    }
}
