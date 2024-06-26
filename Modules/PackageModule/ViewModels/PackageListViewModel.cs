﻿namespace PackageModule.ViewModels;

public class PackageListViewModel : BindableBase, INavigationAware
{
    private readonly ILogger _logger;
    private readonly IDirectoryUtils _directoryUtils;

    private ObservableCollection<Package> _packages = new();
    private Package _selectedPackage = new();
    private bool _isVisible = false;
    private bool _isAllChecked = false;
    private string _nuGetToken = string.Empty;

    public ObservableCollection<Package> Packages
    {
        get => _packages;
        set { SetProperty(ref _packages, value); }
    }

    public string NugetToken
    {
        get => _nuGetToken;
        set 
        { 
            SetProperty(ref _nuGetToken, value);
            if (string.IsNullOrEmpty(_nuGetToken)) 
            { 
                Refresh();
            }
        }
    }

    public Package SelectedPackage
    {
        get => _selectedPackage;
        set 
        { 
            SetProperty(ref _selectedPackage, value); 
            if(_selectedPackage != null) 
            {
                Package? package = Packages.ToList().FirstOrDefault(x => x.Name == _selectedPackage.Name);
                if (package != null) 
                { 
                    package.IsChecked = !_selectedPackage.IsChecked;                  
                }
            }

            IsVisible = Packages.Any(x => x.IsChecked);
        }
    }

    public bool IsAllChecked
    {
        get => _isAllChecked;
        set 
        { 
            SetProperty(ref _isAllChecked, value);
            Packages.ToList().ForEach(x => x.IsChecked = _isAllChecked);
            IsVisible = Packages.Any(x => x.IsChecked);
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set { SetProperty(ref _isVisible, value); }
    }

    public DelegateCommand<string> NugetSearchCommand { get; private set; }
    public DelegateCommand DeleteCommand { get; private set; }

    public PackageListViewModel(ILogger logger, IDirectoryUtils directoryUtils)
    {
        _logger = logger;
        _directoryUtils = directoryUtils;

        NugetSearchCommand = new DelegateCommand<string>(OnNugetSearch);
        DeleteCommand = new DelegateCommand(OnDelete);
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        Refresh();
    }

    private void Refresh() 
    {
        IsVisible = false;
        Packages.Clear();

        List<(string, string?)> packages = _directoryUtils.GetNugetPackages();

        foreach ((string fileName, string? version) in packages) 
        {
            Packages.Add(new Package(fileName, version));
        } 
    }

    private void OnNugetSearch(string token) 
    {
        if (string.IsNullOrEmpty(token)) 
        {
            return;
        }

        Packages.Clear();

        List<(string, string?)> filterPackages = _directoryUtils.GetNugetFilterPackages(token);

        foreach ((string filterPackage, string? version) in filterPackages) 
        {
            Packages.Add(new Package(filterPackage, version));
        }
    }

    private void OnDelete() 
    {
        if (!Packages.Any(x => x.IsChecked)) 
        {
            _logger.Warning("Unable to delete the packages due to no package is selected.");
            return;
        }

        List<string> deletePackages = new();

        foreach (Package package in Packages) 
        {
            if (package.IsChecked) 
            {
                deletePackages.Add(package.Name);
            }
        }

        _directoryUtils.Delete(deletePackages, ConstantFilePaths.NugetX86FilePath);

        Refresh();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}
