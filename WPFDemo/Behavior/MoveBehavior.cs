using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interactivity;

namespace WPFDemo.Behavior
{
    //std name convention when extending Behavior is (class name)Behavior
    // every xaml object which have a Command property implements ICommandSource
    public class MoveBehavior : Behavior<FrameworkElement>, ICommandSource
    {
        /*This class is ment to be an inspiration of how to use behaviors
        To seperate complicated eventhandling,usually when multiple eventhandlers is requred for an action, from the view and viewmodel logic.
        Behaviors is in generel designed to express a desired action and should be generic for its purpose, meaning it could be attached to any frameworkelemnt and execute the action.

            for this particular behavior a few fetures have been left out for you to implement, if you want to use this approach. it have been created like this so you are forced to build upon it.

            currently lacking feature support:
            -   this behavior do not support creation of lines, as each behavior classshould only have one generel purpose.
                use this class as a reference point for creating a new behavior AddLineBehavior.
            
            -   Only Work if there is a parent which is a Canvas (not very difficult to make movebehavior to support it
            
        */

        #region Dependency Propertis  -------------------------------------------

        public static readonly DependencyProperty CanvasTopProperty;
        public static readonly DependencyProperty CanvasLeftProperty;
        public static readonly DependencyProperty IsMoveEnabledProperty;
        public static readonly DependencyProperty OldCanvasLeftProperty;
        public static readonly DependencyProperty OldCanvasTopProperty;

        // create dependency properties for ICommandSource implementation for xaml use
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty CommandParameterProperty;
        public static readonly DependencyProperty CommandTargetProperty;

        public bool IsMoveEnabled
        {
            get { return (bool)GetValue(IsMoveEnabledProperty); }
            set { SetValue(IsMoveEnabledProperty, value); }
        }
        public double CanvasLeft
        {
            get { return (double)GetValue(CanvasLeftProperty); }
            set { SetValue(CanvasLeftProperty, value); }
        }

        public double CanvasTop
        {
            get { return (double)GetValue(CanvasTopProperty); }
            set { SetValue(CanvasTopProperty, value); }
        }
        public double OldCanvasLeft
        {
            get { return (double)GetValue(OldCanvasLeftProperty); }
            set { SetValue(OldCanvasLeftProperty, value); }
        }
        public double OldCanvasTop
        {
            get { return (double)GetValue(OldCanvasTopProperty); }
            set { SetValue(OldCanvasTopProperty, value); }
        }
        // Using a DependencyProperty as the backing store for CanvasTop.  This enables animation, styling, binding, etc...

        private static void OnCanvasPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mob = (MoveBehavior)d;
            if (mob != null && mob.AssociatedObject != null && e.NewValue != e.OldValue)
            {
                if (e.Property.Name == CanvasLeftProperty.Name)
                {
                    mob.AssociatedObject.SetValue(Canvas.LeftProperty, e.NewValue);
                }
                else if (e.Property.Name == CanvasTopProperty.Name)
                {
                    mob.AssociatedObject.SetValue(Canvas.TopProperty, e.NewValue);
                }
            }
        }
        #region ICommandSourceImplementation 


        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public IInputElement CommandTarget
        {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        #region ICommandSource helper methods 
        private void ExecuteCommand()
        {
            // this method will trigger the attached Command defined in the viewModel
            if (this.Command != null)
            {
                RoutedCommand command = Command as RoutedCommand;

                if (command != null)
                {
                    command.Execute(CommandParameter, CommandTarget);
                }
                else
                {
                    ((ICommand)Command).Execute(CommandParameter);
                }
            }
        }
        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var mb = d as MoveBehavior;
            //the if statement is not needed for the application to run as the method when called through the propertyChangedCallback,
            //allways will supply an object reference for d.
            //However the xaml designer sees the method as being static and says d could be null 
            //and therfore d.someMethod could make it crash. this causes the preview render in xaml designer to crash and makes the properties tab empty
            if (mb != null)
            {
                // The HookupCommand makes sure to detach CanExecuteChanged from the old command and attach it to the new command
                mb.HookUpCommand(
                    e.OldValue as ICommand
                    , e.NewValue as ICommand);
            }
        }
        private EventHandler canExecuteChangedHandler;
        private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null)
            {
                RemoveCommand(oldCommand, newCommand);
            }
            AddCommand(oldCommand, newCommand);
        }

        private void RemoveCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = CanExecuteChanged;
            oldCommand.CanExecuteChanged -= handler;
        }
        private void AddCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = new EventHandler(CanExecuteChanged);
            canExecuteChangedHandler = handler;
            if (newCommand != null)
            {
                newCommand.CanExecuteChanged += canExecuteChangedHandler;
            }
        }
        private void CanExecuteChanged(object sender, EventArgs e)
        {
            // whenever the RelayCommand(method,canexecute) asscosiated with this command canexecute option changes this method is called)
            if (this.Command != null)
            {
                RoutedCommand command = this.Command as RoutedCommand;

                // If a RoutedCommand. 
                if (command != null)
                {
                    if (command.CanExecute(CommandParameter, CommandTarget))
                    {
                        this.IsMoveEnabled = true;
                    }
                    else
                    {
                        this.IsMoveEnabled = false;
                    }
                }
                // If a not RoutedCommand. 
                else
                {
                    if (Command.CanExecute(CommandParameter))
                    {
                        this.IsMoveEnabled = true;
                    }
                    else
                    {
                        this.IsMoveEnabled = false;
                    }
                }
            }
        }

        #endregion ICommandSource helper methods
        #endregion ICommandSourceImplementation
        static MoveBehavior()
        {

            CanvasLeftProperty = DependencyProperty.Register("CanvasLeft", typeof(double), typeof(MoveBehavior)
                , new FrameworkPropertyMetadata(BasicBoxes.doubleBox, new PropertyChangedCallback(MoveBehavior.OnCanvasPropertyChanged)) { BindsTwoWayByDefault = true });
            CanvasTopProperty = DependencyProperty.Register("CanvasTop", typeof(double), typeof(MoveBehavior)
                , new FrameworkPropertyMetadata(BasicBoxes.doubleBox, new PropertyChangedCallback(MoveBehavior.OnCanvasPropertyChanged)) { BindsTwoWayByDefault = true });
            OldCanvasLeftProperty = DependencyProperty.Register("OldCanvasLeft", typeof(double), typeof(MoveBehavior), new FrameworkPropertyMetadata(double.NaN) { BindsTwoWayByDefault = true });
            OldCanvasTopProperty = DependencyProperty.Register("OldCanvasTop", typeof(double), typeof(MoveBehavior), new FrameworkPropertyMetadata(double.NaN) { BindsTwoWayByDefault = true });
            IsMoveEnabledProperty = DependencyProperty.Register("IsMoveEnabled", typeof(bool), typeof(MoveBehavior), new PropertyMetadata(BasicBoxes.trueBox));

            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(MoveBehavior), new PropertyMetadata(null, new PropertyChangedCallback(OnCommandChanged)));
            CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(MoveBehavior), new PropertyMetadata(null));
            CommandTargetProperty = DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(MoveBehavior), new PropertyMetadata(null));
        }


        #endregion Dependency Propertis   ---------------------------------------

        #region Attaching

        private Canvas globaCanvas;

        /* when exstending Behavior<> class overriding OnAttached is used
        for initilization of the desired attached element.
        However trying to walk the up the visual tree to a spefic element  
        is not possible as the visual tree for the attached object have not yet been created.
        use the loaded event method instead as it is first called when the visual tree is in place*/
        protected override void OnAttached()
        {

            base.OnAttached();

            this.AssociatedObject.MouseLeftButtonDown += AssociatedObject_MouseLeftButtonDown;
            this.AssociatedObject.MouseMove += AssociatedObject_MouseMove;
            this.AssociatedObject.MouseLeftButtonUp += AssociatedObject_MouseLeftButtonUp;
            this.AssociatedObject.Loaded += AssociatedObject_Loaded;

        }
        // if the parent canvas object never changes for the object it can simply 
        // be declared in the loaded event


        protected override void OnDetaching()
        {
            // if any events where attached during the classes lifetime
            // they can be removed here when the object is being removed
            base.OnDetaching();
            this.AssociatedObject.MouseLeftButtonDown -= AssociatedObject_MouseLeftButtonDown;
            this.AssociatedObject.MouseMove -= AssociatedObject_MouseMove;
            this.AssociatedObject.MouseLeftButtonUp -= AssociatedObject_MouseLeftButtonUp;
            this.AssociatedObject.Loaded -= AssociatedObject_Loaded;
        }

        #endregion Attaching
        #region Event Handlers  -------------------------------------------
        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender == this.AssociatedObject)
            {
                globaCanvas = HelperMethods.FindParentOfType<Canvas>(this.AssociatedObject);
            }

        }

        private Point offset;
        private void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMoveEnabled)
            {
                //first get the position of the associated object relative to its canvas
                offset = e.GetPosition(this.AssociatedObject);
                e.MouseDevice.Target.CaptureMouse();

                if (!OldCanvasLeft.Equals(double.NaN))
                    OldCanvasLeft = CanvasLeft;
                if (!OldCanvasTop.Equals(double.NaN))
                    OldCanvasTop = CanvasTop;
            }

        }
        private void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured != null && IsMoveEnabled)
            {
                //then get the position of the associated object canvas
                //relative to its grid,canvas or window
                Point pos = e.GetPosition(globaCanvas);
                CanvasLeft = pos.X - offset.X;
                CanvasTop = pos.Y - offset.Y;
            }
        }
        private void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMoveEnabled)
            {
                offset = default(Point);
                //atlast trigger the command
                ExecuteCommand();
            }
            e.MouseDevice.Target.ReleaseMouseCapture();
        }
        #endregion Event Handlers   ---------------------------------------
    }
}
