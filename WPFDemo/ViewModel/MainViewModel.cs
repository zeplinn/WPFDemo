using WPFDemo.Command;
using WPFDemo.Model;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WPFDemo.ViewModel
{
    // Use the <summary>...</summary> syntax to create XML Comments, used by Intellisence (Java: Content Assist), 
    // and to generate many types of documentation.
    /// <summary>
    /// This ViewModel is bound to the MainWindow View.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /*
        ------------------IMPORTANT READING------------------

        This Demo is an alternative example to the other Advanced Demo
        showcasing how to describe the move action used by Shape
        with a more "generic approach".

        WARNING: Do not study this version before you understand how the standard Advanced demo is working 
            
            Changes in this Demo version Compared to the other WPFDemos

            new classes added:
                - BasicBoxes
                - HelperMethods
                - MoveBehavior
                - ShapeViewModel
                
             The following Classes have recieved structual changes to make use of ShapeViewModel And MoveBehavior.
                - MainViewModel
                - AddShapeCommand
                - MoveShapeCommand
                - RemoveShapeCommand
                - ShapeUserControl

            the following Xaml-files have had thier databinding updated (ex. from {Binding x} to {Binding Shape.X}).
                - ShapeUserControl
                - App (DataTemplate type Changed to ShapeViewModel)
                - SidePanelUserControl
                https://github.com/zeplinn/WPFDemo.git
        */



        // A reference to the Undo/Redo controller.
        private UndoRedoController undoRedoController = UndoRedoController.Instance;

        // Keeps track of the state, depending on whether a line is being added or not.
        private bool isAddingLine;
        private bool _isMoveable;
        public bool IsMoveable
        {
            get { return _isMoveable; }
            private set { _isMoveable = value; RaisePropertyChanged(); }
        }
        // Used for saving the shape that a line is drawn from, while it is being drawn.
        private Shape addingLineFrom;
      
        

        // Used for making the shapes transparent when a new line is being added.
        // This method uses an expression-bodied member (http://www.informit.com/articles/article.aspx?p=2414582) to simplify a method that only returns a value;
        public double ModeOpacity => isAddingLine ? 0.4 : 1.0;

        // The purpose of using an ObservableCollection instead of a List is that it implements the INotifyCollectionChanged interface, 
        //  which is different from the INotifyPropertyChanged interface.
        // By implementing the INotifyCollectionChanged interface, an event is thrown whenever an element is added or removed from the collection.
        // The INotifyCollectionChanged event is then used by the View, 
        //  which update the graphical representation to show the elements that are now in the collection.
        // Also the collection is generic ("<Type>"), which means that it can be defined to hold all kinds of objects (and primitives), 
        //  but at runtime it is optimized for the specific type and can only hold that type.
        // The "{ get; set; }" syntax describes that a private field 
        //  and default getter setter methods should be generated.
        // This is called Auto-Implemented Properties (http://msdn.microsoft.com/en-us/library/bb384054.aspx).
        public ObservableCollection<ShapeViewModel> Shapes { get; set; }
        public ObservableCollection<Line> Lines { get; set; }

        // Commands that the UI can be bound to.
        // These are read-only properties that can only be set in the constructor.
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        // Commands that the UI can be bound to.
        public ICommand AddShapeCommand { get; }
        public ICommand RemoveShapeCommand { get; }
        public ICommand AddLineCommand { get; }
        public ICommand RemoveLinesCommand { get; }

        // Commands that the UI can be bound to.

        public ICommand MouseUpShapeCommand { get; }

        public MainViewModel()
        {
            IsMoveable = true;
            // Here the list of Shapes is filled with 2 Nodes. 
            // The "new Type() { prop1 = value1, prop2 = value }" syntax is called an Object Initializer, which creates an object and sets its values.
            // Java:
            //  Shape shape1 = new Shape();
            //  shape1.X = 30;
            //  shape1.Y = 40;
            //  shape1.Width = 80;
            //  shape1.Height = 80;
            // Also a constructor could be created for the Shape class that takes the parameters (X, Y, Width and Height), 
            //  and the following could be done:
            // new Shape(30, 40, 80, 80);
            Shapes = new ObservableCollection<ShapeViewModel>() { 
                new ShapeViewModel(new Shape() { X = 30, Y = 40, Width = 80, Height = 80 }), 
                new ShapeViewModel(new Shape() { X = 140, Y = 230, Width = 100, Height = 100 }) 
            };
            // Here the list of Lines i filled with 1 Line that connects the 2 Shapes in the Shapes collection.
            // ElementAt() is an Extension Method, that like many others can be used on all types of collections.
            // It works just like the "Shapes[0]" syntax would be used for arrays.
            Lines = new ObservableCollection<Line>() { 
                new Line() { From = Shapes.ElementAt(0).Shape, To = Shapes.ElementAt(1).Shape } 
            };

            // The commands are given the methods they should use to execute, and find out if they can execute.
            // For these commands the methods are not part of the MainViewModel, but part of the UndoRedoController.
            // Her vidersendes metode kaldne til UndoRedoControlleren.
            UndoCommand = new RelayCommand(undoRedoController.Undo, undoRedoController.CanUndo);
            RedoCommand = new RelayCommand(undoRedoController.Redo, undoRedoController.CanRedo);

            // The commands are given the methods they should use to execute, and find out if they can execute.
            AddShapeCommand = new RelayCommand(AddShape);
            RemoveShapeCommand = new RelayCommand<IList>(RemoveShape, CanRemoveShape);
            AddLineCommand = new RelayCommand(AddLine);
            RemoveLinesCommand = new RelayCommand<IList>(RemoveLines, CanRemoveLines);

            // The commands are given the methods they should use to execute, and find out if they can execute.

            MouseUpShapeCommand = new RelayCommand<MouseButtonEventArgs>(MouseUpShape);
        }

        // Adds a Shape with an AddShapeCommand.
        private void AddShape()
        {
            undoRedoController.AddAndExecute(new AddShapeCommand(Shapes, new Shape()));
        }

        // Checks if the chosen Shapes can be removed, which they can if exactly 1 is chosen.
        // This method uses an expression-bodied member (http://www.informit.com/articles/article.aspx?p=2414582) to simplify a method that only returns a value;
        private bool CanRemoveShape(IList _shapes) => _shapes.Count == 1;

        // Removes the chosen Shapes with a RemoveShapesCommand.
        private void RemoveShape(IList _shapes)
        {
            undoRedoController.AddAndExecute(new RemoveShapesCommand(Shapes, Lines, _shapes.Cast<ShapeViewModel>().ToList()));
        }

        // Starts the procedure to remove a Line, by changing the mode to 'isAddingLine', 
        //  and making the shapes transparent.
        private void AddLine()
        {
            IsMoveable = false;
            isAddingLine = true;
            RaisePropertyChanged(() => ModeOpacity);
        }

        // Checks if the chosen Lines can be removed, which they can if at least one is chosen.
        // This method uses an expression-bodied member (http://www.informit.com/articles/article.aspx?p=2414582) to simplify a method that only returns a value;
        private bool CanRemoveLines(IList _edges) => _edges.Count >= 1;

        // Removes the chosen Lines with a RemoveLinesCommand.
        private void RemoveLines(IList _lines)
        {
            undoRedoController.AddAndExecute(new RemoveLinesCommand(Lines, _lines.Cast<Line>().ToList()));
        }

       
        private void MouseUpShape(MouseButtonEventArgs e)
        {
            // Used for adding a Line.
            if (isAddingLine)
            {
                // Because a MouseUp event has happened and a Line is currently being drawn, 
                //  the Shape that the Line is drawn from or to has been selected, and is here retrieved from the event parameters.
                var shape = TargetShape(e);
                // This checks if this is the first Shape chosen during the Line adding operation, 
                //  by looking at the addingLineFrom variable, which is empty when no Shapes have previously been choosen.
                // If this is the first Shape choosen, and if so, the Shape is saved in the AddingLineFrom variable, 
                //  also the Shape is set as selected, to make it look different visually.
                if (addingLineFrom == null) { addingLineFrom = shape; addingLineFrom.IsSelected = true; }
                // If this is not the first Shape choosen, and therefore the second, 
                //  it is checked that the first and second Shape are different.
                else if (addingLineFrom.Number != shape.Number)
                {
                    // Now that it has been established that the Line adding operation has been completed succesfully by the user, 
                    //  a Line is added using an 'AddLineCommand', with a new Line given between the two shapes chosen.
                    undoRedoController.AddAndExecute(new AddLineCommand(Lines, new Line() { From = addingLineFrom, To = shape }));
                    // The property used for visually indicating that a Line is being Drawn is cleared, 
                    //  so the View can return to its original and default apperance.
                    addingLineFrom.IsSelected = false;
                    // The 'isAddingLine' and 'addingLineFrom' variables are cleared, 
                    //  so the MainViewModel is ready for another Line adding operation.
                    isAddingLine = false;
                    IsMoveable = true;
                    addingLineFrom = null;
                    // The property used for visually indicating which Shape has already chosen are choosen is cleared, 
                    //  so the View can return to its original and default apperance.
                    RaisePropertyChanged(() => ModeOpacity);
                }
            }
            
        }

        // Gets the shape that was clicked.
        private Shape TargetShape(MouseEventArgs e)
        {
            // Here the visual element that the mouse is captured by is retrieved.
            var shapeVisualElement = (FrameworkElement)e.MouseDevice.Target;
            // From the shapes visual element, the Shape object which is the DataContext is retrieved.
            return (Shape)shapeVisualElement.DataContext;
        }

        // Gets the mouse position relative to the canvas.
        private Point RelativeMousePosition(MouseEventArgs e)
        {
            // Here the visual element that the mouse is captured by is retrieved.
            var shapeVisualElement = (FrameworkElement)e.MouseDevice.Target;
            // The canvas holding the shapes visual element, is found by searching up the tree of visual elements.
            var canvas = FindParentOfType<Canvas>(shapeVisualElement);
            // The mouse position relative to the canvas is gotten here.
            return Mouse.GetPosition(canvas);
        }

        // Recursive method for finding the parent of a visual element of a certain type, 
        //  by searching up the visual tree of parent elements.
        // The '() ? () : ()' syntax, returns the second part if the first part is true, otherwise it returns the third part.
        // This uses 'dynamic' which is an dynamic type variable (https://msdn.microsoft.com/en-us/library/dd264736.aspx).
        private static T FindParentOfType<T>(DependencyObject o)
        {
            dynamic parent = VisualTreeHelper.GetParent(o);
            return parent.GetType().IsAssignableFrom(typeof(T)) ? parent : FindParentOfType<T>(parent);
        }
    }
}