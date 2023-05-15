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

        #region Список комнат

        private ObservableCollection<string> _rooms;

        public ObservableCollection<string> Rooms
        {
            get => _rooms;
            set => Set(ref _rooms, value);
        }

        #endregion

        #region Команды

        #region Команда получение всех комнат

        public ICommand GetRoomsCommand { get; }

        private void OnGetRoomsCommandExecuted(object parameter)
        {
            Rooms = new ObservableCollection<string>(RevitModel.GetAllRooms());
        }

        private bool CanGetRoomsCommandExecute(object parameter)
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

            GetRoomsCommand = new LambdaCommand(OnGetRoomsCommandExecuted, CanGetRoomsCommandExecute);

            GetDWGName = new LambdaCommand(OnGetDWGNameExecuted, CanGetDWGNameExecute);

            #endregion
        }
        #endregion
    }
}
