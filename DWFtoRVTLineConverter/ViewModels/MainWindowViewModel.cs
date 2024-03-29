﻿using System;
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

        private string _title = "DWG в RVT";

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

        #region Полилинии
        private List<PolylineUtils> _lines;

        public List<PolylineUtils> Lines
        {
            get => _lines;
            set => Set(ref _lines, value);
        }
        #endregion

        #region Открыть созданный файл семейства с линиями
        private bool _isOpenFamilyInApplication;
        public bool IsOpenFamilyInApplication
        {
            get => _isOpenFamilyInApplication;
            set => Set(ref _isOpenFamilyInApplication, value);
        }
        #endregion

        #region Команды

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

        #region Создание линий в семействе

        public ICommand CreateLinesInFamily { get; }

        private void OnCreateLinesInFamilyCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            Lines = new List<PolylineUtils>(RevitModel.ImportPolyLinesfromJson());
            RevitModel.CreatePolylinesInFamily(Lines);
            RevitCommand.mainView.Close();
        }

        private bool CanCreateLinesInFamilyCommandExecute(object parameter)
        {
            return true;
        }

        #endregion

        #region Создание линий внутри проекта

        public ICommand CreateDirectShapeLines { get; }

        private void OnCreateDirectShapeLinesCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.CreateDirectShapeLinesInModel();
            RevitCommand.mainView.Close();
        }

        private bool CanCreateDirectShapeLinesCommandExecute(object parameter)
        {
            return true;
        }

        #endregion

        #region Сохранение линий в семейство
        public ICommand SaveCurvesInFamilyCommand { get; }

        private void OnSaveCurvesInFamilyCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.SaveCurvesInFamilyFile(IsOpenFamilyInApplication);
            RevitCommand.mainView.Close();
        }

        private bool CanSaveCurvesInFamilyCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Команды

            ExportLinesToJson = new LambdaCommand(OnExportLinesToJsonCommandExecuted, CanExportLinesToJsonCommandExecute);

            CreateLinesInFamily = new LambdaCommand(OnCreateLinesInFamilyCommandExecuted, CanCreateLinesInFamilyCommandExecute);

            SaveCurvesInFamilyCommand = new LambdaCommand(OnSaveCurvesInFamilyCommandExecuted, CanSaveCurvesInFamilyCommandExecute);

            CreateDirectShapeLines = new LambdaCommand(OnCreateDirectShapeLinesCommandExecuted, CanCreateDirectShapeLinesCommandExecute);

            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
