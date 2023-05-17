using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using System.Windows.Input;
using DWGtoRVTLineConverter.Infrastructure;
using DWGtoRVTLineConverter.Models;
using System.Windows.Shapes;

namespace DWGtoRVTLineConverter.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок

        private string _title = "DWG to RVT";

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }

        #endregion

        #region Имя DWG файла

        private string _dwgFileName;

        public string DwgFileName
        {
            get => _dwgFileName;
            set => Set(ref _dwgFileName, value);
        }

        #endregion

        #region Список линий

        private ObservableCollection<string> _linesname;

        public ObservableCollection<string> LinesName
        {
            get => _linesname;
            set => Set(ref _linesname, value);
        }

        #endregion

        #region Полилинии

        private List<PolylineUtils> _lines;

        public List<PolylineUtils> Lines
        {
            get => _lines;
            set => Set(ref _lines, value);
        }


        #endregion

        #region Команды

        #region Команда получение всех линий

        public ICommand GetPolyLines { get; }

        private void OnGetPolyLinesCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            LinesName = new ObservableCollection<string>(RevitModel.GetAllPolyLinesName());
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetPolyLinesCommandExecute(object parameter)
        {
            return true;
        }

        #endregion

        #region Экспорт линий в Json файл

        public ICommand ExportLinesToJson { get; }

        private void OnExportLinesToJsonCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            string fileName = string.Empty;
            Lines = new List<PolylineUtils>(RevitModel.GetAllPolyLinesFromDWG(ref fileName));
            RevitModel.ExportPolyLines(Lines);
            DwgFileName = fileName;
            RevitCommand.mainView.ShowDialog();

        }

        private bool CanExportLinesToJsonCommandExecute(object parameter)
        {
            return true;
        }

        #endregion

        #region Импорт точек из полилиний из Json файла

        public ICommand CreatePointsFromJson { get; }

        private void OnCreatePointsFromJsonCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            Lines = new List<PolylineUtils>(RevitModel.ImportPolyLinesfromJson());
            foreach(var line in Lines)
            {
                RevitModel.CreateAdaptivePoints(line);
            }
        }

        private bool CanCreatePointsFromJsonCommandExecute(object parameter)
        {
            return true;
        }

        #endregion

        #region Создание линий в семействе

        public ICommand CreateLinesInFamily { get; }

        public void OnCreateLinesInFamilyCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            Lines = new List<PolylineUtils>(RevitModel.ImportPolyLinesfromJson());
            RevitModel.CreatePolylinesInFamily(Lines);
        }

        public bool CanCreateLinesInFamilyCommandExecute(object parameter)
        {
            return true;
        }

        #endregion

        #region Получение имени DWG файла

        public ICommand GetDWGName { get; }

        private void OnGetDWGNameExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            DwgFileName = RevitModel.GetDWGFileName();
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetDWGNameExecute(object parameter)
        {
            return true;
        }

        #endregion

        #endregion


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel()
        {
            #region Команды

            GetPolyLines = new LambdaCommand(OnGetPolyLinesCommandExecuted, CanGetPolyLinesCommandExecute);

            GetDWGName = new LambdaCommand(OnGetDWGNameExecuted, CanGetDWGNameExecute);

            ExportLinesToJson = new LambdaCommand(OnExportLinesToJsonCommandExecuted, CanExportLinesToJsonCommandExecute);

            CreatePointsFromJson = new LambdaCommand(OnCreatePointsFromJsonCommandExecuted, CanCreatePointsFromJsonCommandExecute);

            CreateLinesInFamily = new LambdaCommand(OnCreateLinesInFamilyCommandExecuted, CanCreateLinesInFamilyCommandExecute);

            #endregion
        }
        #endregion
    }
}
