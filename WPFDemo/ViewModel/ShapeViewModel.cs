using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFDemo.Command;
using WPFDemo.Model;

namespace WPFDemo.ViewModel
{
    public class ShapeViewModel : ViewModelBase
    {
        // a viewModel wrapper class for Shape
        #region Shape Wrapper 
        private Shape _shape;

        public Shape Shape
        {
            get { return _shape; }
            private set { _shape = value; RaisePropertyChanged(); }
        }

        // The Shape ViewModel Should Allways have a shape
        public ShapeViewModel() : this(new Shape()) { }

        public ShapeViewModel(Shape shape)
        {
            Shape = shape;
        }
        public override string ToString() => Shape.ToString();

        #endregion Shape Wrapper

        #region properties used for MoveBehaior 

        public ICommand MoveCommand
        {
            get { return new RelayCommand(ExecuteMoveCommand); }
        }
        private double _oldX;
        private double _oldY;
        public double OldX
        {
            get { return _oldX; }
            set { _oldX = value; RaisePropertyChanged(); }
        }


        public double OldY
        {
            get { return _oldY; }
            set { _oldY = value; RaisePropertyChanged(); }
        }

        private void ExecuteMoveCommand()
        {
            var xProxy = Shape.X;
            var yProxy = Shape.Y;
            Shape.X = OldX;
            Shape.Y = OldY;
            UndoRedoController.Instance.AddAndExecute(new MoveShapeCommand(Shape, xProxy - OldX, yProxy - OldY));
        }
        #endregion properties used for MoveBehaior
    }
}
