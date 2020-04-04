using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Ink;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System;

namespace Практика2Курс
{
    public abstract class Phone : INotifyPropertyChanged
    {
        private string name;
        private string manufactorName;
        private float scale;

        protected float width;
        protected float height;
        protected Canvas phoneField;
        protected Canvas originalPhone;
        protected bool mouseIsDown;
        protected bool isDrawed;
        protected bool isCopyMode;
        protected bool isAnimating;

        public bool IsSelected { get; set; }
        public Canvas PaintField { get; set; }
        public Color PenColor { get; set; }

        public float Height { get { return height; } }
        public float Width { get { return width; } }
        public Canvas PhoneField { get { return phoneField; } }

        public Phone() : this(null, null, Colors.Black)
        { }

        public Phone(string name, Canvas mainPaintField, Color penColor)
        {
            Name = name;
            PaintField = mainPaintField;

            height = 150;
            width = 99;
            Scale = 1f;
            isDrawed = false;
            mouseIsDown = false;
            isAnimating = false;
            PenColor = penColor;

            phoneField = new Canvas();
            originalPhone = null;
            IsSelected = true;

            Canvas.SetLeft(phoneField, 0);
            Canvas.SetTop(phoneField, 0);

            phoneField.Width = Width;
            phoneField.Height = Height;

            phoneField.PreviewMouseDown += (downObj, downArg) =>
            {
                mouseIsDown = true;
            };

            phoneField.PreviewMouseUp += (upObj, upArg) =>
            {
                mouseIsDown = false;
                IsSelected = true;
            };

            phoneField.PreviewMouseMove += (moveObj, moveArg) =>
            {
                int index = PaintField.Children.IndexOf(phoneField);

                if (index >= 0 && mouseIsDown && phoneField.IsMouseOver)
                {
                    phoneField = (Canvas)PaintField.Children[index];

                    Move(moveArg.GetPosition(PaintField));
                }
            };

            phoneField.PreviewMouseWheel += (wheelObj, wheelArg) =>
            {
                if (!phoneField.IsMouseOver || isAnimating)
                {
                    return;
                }

                if (wheelArg.Delta > 0)
                {
                    Scale += 0.1f;
                }
                else if (wheelArg.Delta < 0)
                {
                    Scale -= 0.1f;
                }

                Draw();
            };
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public string ManufactorName
        {
            get { return manufactorName; }
            set
            {
                manufactorName = value;
                OnPropertyChanged("ManufactorName");
            }
        }
        public float Scale
        {
            get { return scale; }
            set
            {
                if (value > 0.1)
                {
                    scale = value;
                }
                else
                {
                    scale = 0.1f;
                }
                OnPropertyChanged("Scale");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract void Draw();

        //Виртуальный метод для анимации, который ничего не делает 
        //(он может быть не всем нужен, так что его не обязательно переопределять)
        public virtual void Animation() { }

        //Двигает объект (берёт новую координату и ставит её как верхний левый угол)
        public void Move(Point newPoint)
        {

            if (newPoint.Y - (Height * Scale) / 2 < 0)
            {
                newPoint.Y = 0;
            }
            else if (newPoint.Y + (Height * Scale) / 2 > PaintField.ActualHeight)
            {
                newPoint.Y = PaintField.ActualHeight - Height * Scale;
            }
            else
            {
                newPoint.Y -= (Height * Scale) / 2;
            }

            if (newPoint.X - (Width * Scale) / 2 < 0)
            {
                newPoint.X = 0;
            }
            else if (newPoint.X + (Width * Scale) / 2 > PaintField.ActualWidth)
            {
                newPoint.X = PaintField.ActualWidth - Width * Scale;
            }
            else
            {
                newPoint.X -= (Width * Scale) / 2;
            }

            Canvas.SetTop(phoneField, newPoint.Y);
            Canvas.SetLeft(phoneField, newPoint.X);
        }

        //Вставляет оригинальный телефон (без изменения размера) в канвас, который передают
        public void CopyOriginalPhoneTo(Canvas destinationField)
        {
            //Добавление происходит в children, потому что при приравнивании теряется ссылка
            if (originalPhone != null)
            {
                destinationField.Children.Clear();
                destinationField.Children.Add(originalPhone);
                return;
            }

            destinationField.Children.Clear();
            originalPhone = new Canvas();
            isCopyMode = true;
            Canvas oldPaintField = PaintField;
            PaintField = destinationField;

            Draw();

            PaintField = oldPaintField;
            isCopyMode = false;
        }

        //Нужно для привязки текста к компонентам на форме (оно возникает при изменении свойства)
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public override string ToString()
        {
            string text = "";

            text += "Имя: " + name;
            text += "Имя производителя: " + manufactorName;
            text += "Размер (высота х ширина): " + $"{height * Scale}x{width * Scale}";

            return text;
        }

    }

    // :)
    public class SmartPhone : Phone
    {
        public StrokeCollection PhoneStrokes { get; set; }
        
        public SmartPhone() : this(null, null, Colors.Black)
        { }

        public SmartPhone(string name, Canvas paintField, Color penColor) : base(name, paintField, penColor)
        {
            PhoneStrokes = new StrokeCollection();
        }

        public override void Draw()
        {

            if (phoneField == null)
            {
                MessageBox.Show("Не возможно нарисовать телефон");
                return;
            }

            //Ищет у бирает текущее поле телефона в общем поле и чистит текущее
            if (isDrawed && !isCopyMode)
            {
                int index = PaintField.Children.IndexOf(phoneField);

                if (index >= 0)
                {
                    phoneField = (Canvas)PaintField.Children[index];

                    PaintField.Children.Remove(phoneField);

                    phoneField.Children.Clear();

                    phoneField.Width = Width * Scale;
                    phoneField.Height = Height * Scale;
                }
            }


            float oldScale = -1f;

            //Создаёт отдельный рисунок для примера
            if (isCopyMode)
            {
                oldScale = Scale;
                Scale = 1;
            }

            Path borderPath = new Path();

            borderPath.Data = new RectangleGeometry(new Rect(2 * Scale, 2 * Scale, 94 * Scale, 146 * Scale), 15, 15);
            borderPath.Width = Width * Scale;
            borderPath.Height = Height * Scale;
            borderPath.Stroke = new SolidColorBrush(PenColor);
            borderPath.StrokeThickness = 6;

            //Внутрнееполе
            Path mainPath = new Path();
            GeometryGroup geometryGroup = new GeometryGroup();

            //Начало кнопок принятия и отклонения звонка

            PathGeometry pathGeometry = new PathGeometry();

            PathFigure acceptButton = new PathFigure(new Point(15 * Scale, 133 * Scale), new List<PathSegment>
            {
                new ArcSegment(new Point(26 * Scale, 133 * Scale), new Size(3, 2), 0, false, SweepDirection.Clockwise, true),
            }, false);

            PathFigure refuseButton = new PathFigure(new Point(69 * Scale, 133 * Scale), new List<PathSegment>
            {
                new ArcSegment(new Point(80 * Scale, 133 * Scale), new Size(3, 2), 0, false, SweepDirection.Clockwise, true),
            }, false);

            //Конец кнопок принятия и отклонения звонка
            pathGeometry.Figures.Add(acceptButton);
            pathGeometry.Figures.Add(refuseButton);
            geometryGroup.Children.Add(pathGeometry);

            geometryGroup.Children.Add(new EllipseGeometry(new Point(74.6 * Scale, 134 * Scale), 3 * Scale, 3 * Scale));

            geometryGroup.Children.Add(new RectangleGeometry(new Rect(13 * Scale, 125 * Scale, 15 * Scale, 15 * Scale), 5, 5));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(67 * Scale, 125 * Scale, 15 * Scale, 15 * Scale), 5, 5));

            isDrawed = true;

            mainPath.Data = geometryGroup;
            mainPath.Width = Width * Scale;
            mainPath.Height = Height * Scale;
            mainPath.Stroke = new SolidColorBrush(PenColor);

            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("PhoneStrokes");
            binding.Mode = BindingMode.TwoWay;

            InkCanvas inkCanvas = new InkCanvas();
            inkCanvas.EditingMode = InkCanvasEditingMode.None;
            inkCanvas.Width = 84 * Scale;
            inkCanvas.Height = 110 * Scale;
            inkCanvas.SetBinding(InkCanvas.StrokesProperty, binding);
            


            if (isCopyMode)
            {

                Canvas.SetTop(inkCanvas, 7);
                Canvas.SetLeft(inkCanvas, 7);

                Button drawButton = new Button();
                Button eraseButton = new Button();
                Button animationButton = new Button();

                drawButton.Width = 60;
                drawButton.Height = 30;
                drawButton.Content = "Рисовать";
                drawButton.Click += (o, e) =>
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                };

                eraseButton.Width = 60;
                eraseButton.Height = 30;
                eraseButton.Content = "Стирать";
                eraseButton.Click += (o, e) =>
                {
                    inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                };

                animationButton.Width = 140;
                animationButton.Height = 30;
                animationButton.Content = "Включить анимацию";
                animationButton.Click += (o, e) =>
                {
                    Animation();
                };

                Canvas.SetLeft(drawButton, -20);
                Canvas.SetTop(drawButton, 155);
                Canvas.SetLeft(eraseButton, 60);
                Canvas.SetTop(eraseButton, 155);
                Canvas.SetLeft(animationButton, -20);
                Canvas.SetTop(animationButton, 195);

                originalPhone.Children.Add(inkCanvas);
                originalPhone.Children.Add(borderPath);
                originalPhone.Children.Add(mainPath);
                originalPhone.Children.Add(drawButton);
                originalPhone.Children.Add(eraseButton);
                originalPhone.Children.Add(animationButton);

                PaintField.Children.Add(originalPhone);
            }
            else
            {

                Button trigger = new Button();
                trigger.Width = Width * Scale;
                trigger.Height = Height * Scale;
                trigger.Opacity = 0;


                inkCanvas.Strokes = PhoneStrokes;

                phoneField.Children.Add(inkCanvas);
                phoneField.Children.Add(borderPath);
                phoneField.Children.Add(mainPath);
                phoneField.Children.Add(trigger);


                PaintField.Children.Add(phoneField);

            }

        }

        public override void Animation()
        {
            isAnimating = true;

            Path ellipsePath = new Path();
            Path ellipseoriginalPhonePath = new Path();
            EllipseGeometry ellipse = new EllipseGeometry(new Point((Width * Scale) / 2, (Height * Scale) / 2), 0, 0);
            EllipseGeometry ellipseOriginalPhone = new EllipseGeometry(new Point((Width) / 2, (Height) / 2), 0, 0);

            DoubleAnimation radiusXAnimation = new DoubleAnimation();
            DoubleAnimation radiusYAnimation = new DoubleAnimation();
            DoubleAnimation radiusXOriginalPhoneAnimation = new DoubleAnimation();
            DoubleAnimation radiusYOriginalPhoneAnimation = new DoubleAnimation();

            radiusXAnimation.From = 0;
            radiusXAnimation.To = Width * Scale - 2 * Scale;
            radiusXAnimation.Duration = new Duration(new TimeSpan(0, 0, 5));
            radiusXAnimation.Completed += (obj, arg) =>
            {
                phoneField.Children.Remove(ellipsePath);
                isAnimating = false;
            };

            radiusYAnimation.From = 0;
            radiusYAnimation.To = Height * Scale - 10 * Scale;
            radiusYAnimation.Duration = new Duration(new TimeSpan(0, 0, 5));

            radiusXOriginalPhoneAnimation.From = 0;
            radiusXOriginalPhoneAnimation.To = Width * Scale - 2 * Scale;
            radiusXOriginalPhoneAnimation.Duration = new Duration(new TimeSpan(0, 0, 5));
            radiusXOriginalPhoneAnimation.Completed += (obj, arg) =>
            {
                originalPhone.Children.Remove(ellipsePath);
            };

            radiusYOriginalPhoneAnimation.From = 0;
            radiusYOriginalPhoneAnimation.To = Height * Scale - 10 * Scale;
            radiusYOriginalPhoneAnimation.Duration = new Duration(new TimeSpan(0, 0, 5));

            ellipseoriginalPhonePath.Data = ellipseOriginalPhone;
            ellipseoriginalPhonePath.Height = Height;
            ellipseoriginalPhonePath.Width = Width;
            ellipseoriginalPhonePath.Stroke = new SolidColorBrush(PenColor);

            ellipsePath.Data = ellipse;
            ellipsePath.Height = Height * Scale;
            ellipsePath.Width = Width * Scale;
            ellipsePath.Stroke = new SolidColorBrush(PenColor);

            ellipse.BeginAnimation(EllipseGeometry.RadiusXProperty, radiusXAnimation);
            ellipse.BeginAnimation(EllipseGeometry.RadiusYProperty, radiusYAnimation);
            ellipseOriginalPhone.BeginAnimation(EllipseGeometry.RadiusXProperty, radiusXAnimation);
            ellipseOriginalPhone.BeginAnimation(EllipseGeometry.RadiusYProperty, radiusYAnimation);

            phoneField.Children.Add(ellipsePath);
            originalPhone.Children.Add(ellipseoriginalPhonePath);
        }
    }

    public abstract class PushButtonPhone : Phone
    {
        private string text;

        public PushButtonPhone() : base()
        { }

        public PushButtonPhone(string name, Canvas paintField, Color penColor) : base(name, paintField, penColor)
        {
            Text = "";
        }

        public string Text
        {
            get { return text; }
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

    }

    //Просто кнопочный телефон
    public class NormalPushButtonPhone : PushButtonPhone
    {
        private int lifeTime;
        private DispatcherTimer lifeTimer;

        public int CurrentLifeTime
        {
            get { return lifeTime; }
            private set
            {
                lifeTime = value;
                OnPropertyChanged("CurrentLifeTime");
            }
        }
        public DispatcherTimer LifeTimer { get { return lifeTimer; } }
        public NormalPushButtonPhone() : this(null, null, Colors.Black)
        { }
        public NormalPushButtonPhone(string name, Canvas paintField, Color penColor) : base(name, paintField, penColor)
        {
            lifeTime = 100;
            lifeTimer = new DispatcherTimer();
            lifeTimer.Interval = new System.TimeSpan(0, 0, 1);
            lifeTimer.Tick += (o, e) =>
            {
                if (CurrentLifeTime > 0)
                {
                    CurrentLifeTime -= 1;
                }
                else
                {
                    PaintField.Children.Remove(phoneField);
                    lifeTimer.Stop();
                }

            };
            lifeTimer.Start();
        }

        public override void Draw()
        {
            if (phoneField == null)
            {
                MessageBox.Show("Не возможно нарисовать телефон");
                return;
            }

            //Ищет у бирает текущее поле телефона в общем поле и чистит текущее
            if (isDrawed && !isCopyMode)
            {
                int index = PaintField.Children.IndexOf(phoneField);

                if (index >= 0)
                {
                    phoneField = (Canvas)PaintField.Children[index];

                    PaintField.Children.Remove(phoneField);

                    phoneField.Children.Clear();

                    phoneField.Width = Width * Scale;
                    phoneField.Height = Height * Scale;
                }
            }

            float oldScale = -1f;

            //Создаёт отдельный рисунок для примера
            if (isCopyMode)
            {
                oldScale = Scale;
                Scale = 1;
            }

            Path mainPath = new Path();
            GeometryGroup geometryGroup = new GeometryGroup();

            //Начало кнопок принятия и отклонения звонка

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure acceptButton = new PathFigure(new Point(10 * Scale, 82 * Scale), new List<PathSegment>
            {
                new ArcSegment(new Point(23 * Scale, 82 * Scale), new Size(10, 9), 0, false, SweepDirection.Clockwise, true),
            }, false);

            PathFigure refuseButton = new PathFigure(new Point(77 * Scale, 82 * Scale), new List<PathSegment>
            {
                new ArcSegment(new Point(90 * Scale, 82 * Scale), new Size(10, 9), 0, false, SweepDirection.Clockwise, true),
            }, false);

            pathGeometry.Figures.Add(acceptButton);
            pathGeometry.Figures.Add(refuseButton);
            geometryGroup.Children.Add(pathGeometry);

            //Конец кнопок принятия и отклонения звонка

            //Начало цыфер и доп знаков

            //1
            geometryGroup.Children.Add(new LineGeometry(new Point(15 * Scale, 92 * Scale), new Point(15 * Scale, 103 * Scale)));

            //2
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 92 * Scale), new Point(55 * Scale, 92 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 92 * Scale), new Point(55 * Scale, 97 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 97 * Scale), new Point(55 * Scale, 97 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 97 * Scale), new Point(45 * Scale, 103 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 103 * Scale), new Point(55 * Scale, 103 * Scale)));

            //3
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 92 * Scale), new Point(90 * Scale, 92 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 92 * Scale), new Point(90 * Scale, 97 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 97 * Scale), new Point(90 * Scale, 97 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 97 * Scale), new Point(90 * Scale, 103 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 103 * Scale), new Point(90 * Scale, 103 * Scale)));

            //4
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 112 * Scale), new Point(20 * Scale, 112 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 107 * Scale), new Point(10 * Scale, 112 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(20 * Scale, 107 * Scale), new Point(20 * Scale, 117 * Scale)));

            //5
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 107 * Scale), new Point(55 * Scale, 107 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 107 * Scale), new Point(45 * Scale, 112 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 112 * Scale), new Point(55 * Scale, 112 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 112 * Scale), new Point(55 * Scale, 117 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 117 * Scale), new Point(55 * Scale, 117 * Scale)));

            //6
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 107 * Scale), new Point(90 * Scale, 107 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 107 * Scale), new Point(78 * Scale, 117 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 112 * Scale), new Point(90 * Scale, 112 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 112 * Scale), new Point(90 * Scale, 117 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 117 * Scale), new Point(90 * Scale, 117 * Scale)));

            //7
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 122 * Scale), new Point(20 * Scale, 122 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(20 * Scale, 132 * Scale), new Point(20 * Scale, 122 * Scale)));

            //8
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 122 * Scale), new Point(55 * Scale, 122 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 122 * Scale), new Point(45 * Scale, 132 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 127 * Scale), new Point(55 * Scale, 127 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 122 * Scale), new Point(55 * Scale, 132 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 132 * Scale), new Point(55 * Scale, 132 * Scale)));

            //9
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 122 * Scale), new Point(90 * Scale, 122 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 122 * Scale), new Point(78 * Scale, 127 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 127 * Scale), new Point(90 * Scale, 127 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 122 * Scale), new Point(90 * Scale, 132 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 132 * Scale), new Point(90 * Scale, 132 * Scale)));

            //0
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 137 * Scale), new Point(55 * Scale, 137 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 137 * Scale), new Point(45 * Scale, 147 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 137 * Scale), new Point(55 * Scale, 147 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 147 * Scale), new Point(55 * Scale, 147 * Scale)));

            //*
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 137 * Scale), new Point(20 * Scale, 147 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(15 * Scale, 137 * Scale), new Point(15 * Scale, 147 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 142 * Scale), new Point(20 * Scale, 142 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 147 * Scale), new Point(20 * Scale, 137 * Scale)));

            //#
            geometryGroup.Children.Add(new LineGeometry(new Point(76 * Scale, 140 * Scale), new Point(91 * Scale, 140 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(80 * Scale, 137 * Scale), new Point(78 * Scale, 147 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(75 * Scale, 145 * Scale), new Point(90 * Scale, 145 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(86 * Scale, 147 * Scale), new Point(88 * Scale, 137 * Scale)));

            //Окончание цыфер и доп знаков

            //Просто кружки))
            geometryGroup.Children.Add(new EllipseGeometry(new Point(84 * Scale, 83 * Scale), 2 * Scale, 2 * Scale));
            geometryGroup.Children.Add(new EllipseGeometry(new Point(50 * Scale, 75 * Scale), 10 * Scale, 10 * Scale));

            //Линии на кнопках
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 67 * Scale), new Point(23 * Scale, 67 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(77 * Scale, 67 * Scale), new Point(90 * Scale, 67 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(33 * Scale, 90 * Scale), new Point(33 * Scale, 150 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(66 * Scale, 90 * Scale), new Point(66 * Scale, 150 * Scale)));

            //Сами кнопки телефона
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(33 * Scale, 60 * Scale, 33 * Scale, 30 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 60 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 75 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(66 * Scale, 60 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(66 * Scale, 75 * Scale, 33 * Scale, 15 * Scale)));


            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 90 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 105 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 120 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 135 * Scale, 33 * Scale, 15 * Scale)));

            geometryGroup.Children.Add(new RectangleGeometry(new Rect(33 * Scale, 90 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(33 * Scale, 105 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(33 * Scale, 120 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(33 * Scale, 135 * Scale, 33 * Scale, 15 * Scale)));

            geometryGroup.Children.Add(new RectangleGeometry(new Rect(66 * Scale, 90 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(66 * Scale, 105 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(66 * Scale, 120 * Scale, 33 * Scale, 15 * Scale)));
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(66 * Scale, 135 * Scale, 33 * Scale, 15 * Scale)));

            //Рамка телефона
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 0 * Scale, 99 * Scale, 150 * Scale)));

            mainPath.Data = geometryGroup;
            mainPath.Width = Width * Scale;
            mainPath.Height = Height * Scale;
            mainPath.Stroke = new SolidColorBrush(PenColor);
            mainPath.StrokeThickness = 2;

            //Поле для вывода символов
            TextBlock textBlock = new TextBlock();
            textBlock.Width = Width * Scale;
            textBlock.Height = Height * Scale * 0.4;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.FontSize *= Scale;
            Canvas.SetTop(textBlock, 11 * Scale);

            ProgressBar progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Width = 40 * Scale;
            progressBar.Height = 6 * Scale;
            Canvas.SetTop(progressBar, 3);
            Canvas.SetLeft(progressBar, Width * Scale - (Width * Scale / 2));

            Binding lifeTimeBind = new Binding();
            lifeTimeBind.Source = this;
            lifeTimeBind.Path = new PropertyPath("CurrentLifeTime");
            lifeTimeBind.Mode = BindingMode.OneWay;
            progressBar.SetBinding(ProgressBar.ValueProperty, lifeTimeBind);

            //Привязка к текста к полю с текстом 
            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("Text");
            binding.Mode = BindingMode.TwoWay;
            textBlock.SetBinding(TextBlock.TextProperty, binding);

            if (isCopyMode)
            {
                Button[] buttons = new Button[]
                {
                    new Button(), new Button(), new Button(), new Button(), new Button(), new Button(),
                    new Button(), new Button(), new Button(), new Button(), new Button(), new Button()
                };

                originalPhone.Children.Add(mainPath);

                for (int i = 0, count = 0, top = 90; i < buttons.Length; i++, count++)
                {

                    if (count > 2)
                    {
                        count = 0;
                        top += 15;
                    }

                    buttons[i].Width = 33;
                    buttons[i].Height = 15;
                    buttons[i].Opacity = 0;
                    Canvas.SetLeft(buttons[i], count * 33);
                    Canvas.SetTop(buttons[i], top);


                    if (i < 9)
                    {
                        int ind = i + 1;
                        buttons[i].Click += (o, e) =>
                        {
                            Text += ind.ToString();
                        };
                    }
                    else if (i == 9)
                    {
                        buttons[i].Click += (o, e) =>
                        {
                            Text += "*";
                        };
                        buttons[i + 1].Click += (o, e) =>
                        {
                            Text += "0";
                        };
                        buttons[i + 2].Click += (o, e) =>
                        {
                            Text += "#";
                        };
                    }

                    originalPhone.Children.Add(buttons[i]);
                }

                Button deleteButton = new Button();
                Button deleteAllButton = new Button();

                deleteButton.Width = 33;
                deleteButton.Height = 15;
                Canvas.SetLeft(deleteButton, 66);
                Canvas.SetTop(deleteButton, 60);

                deleteAllButton.Width = 33;
                deleteAllButton.Height = 15;
                Canvas.SetLeft(deleteAllButton, 66);
                Canvas.SetTop(deleteAllButton, 75);
                deleteAllButton.Opacity = 0;
                deleteButton.Opacity = 0;

                deleteAllButton.Click += (o, e) =>
                {
                    Text = "";
                };

                deleteButton.Click += (o, e) =>
                {
                    if (Text.Length > 0)
                    {
                        Text = Text.Remove(Text.Length - 1);
                    }
                };

                Scale = oldScale;

                originalPhone.Children.Add(progressBar);
                originalPhone.Children.Add(deleteButton);
                originalPhone.Children.Add(deleteAllButton);
                originalPhone.Children.Add(textBlock);

                PaintField.Children.Add(originalPhone);

            }
            else
            {
                Button trigger = new Button();

                trigger.Width = Width * Scale;
                trigger.Height = Height * Scale;
                trigger.Opacity = 0;

                phoneField.Children.Add(progressBar);
                phoneField.Children.Add(mainPath);
                phoneField.Children.Add(textBlock);
                phoneField.Children.Add(trigger);

                PaintField.Children.Add(phoneField);

                isDrawed = true;
            }

        }
    }

    //Раздвижной телефон
    public class RetractablePushButtonPhone : PushButtonPhone
    {
        //Служит куском, который будет задвигаться/выдвигаться
        private Canvas animatedPiece;
        private Canvas animatedOriginalPhonePiece;
        private bool isHidden;

        public RetractablePushButtonPhone() : this(null, null, Colors.Black)
        { }

        public RetractablePushButtonPhone(string name, Canvas paintFIeld, Color penColor) : base(name, paintFIeld, penColor)
        {
            width = 99;
            height = 210;
            isHidden = false;

            animatedPiece = new Canvas();
            animatedOriginalPhonePiece = new Canvas();
        }
        public override void Draw()
        {

            if (phoneField == null)
            {
                MessageBox.Show("Не возможно нарисовать телефон");
                return;
            }

            //Ищет у бирает текущее поле телефона в общем поле и чистит текущее
            if (isDrawed && !isCopyMode)
            {
                int index = PaintField.Children.IndexOf(phoneField);

                if (index >= 0)
                {
                    phoneField = (Canvas)PaintField.Children[index];

                    PaintField.Children.Remove(phoneField);

                    phoneField.Children.Clear();

                    phoneField.Width = Width * Scale;
                    phoneField.Height = Height * Scale;

                    animatedPiece.Children.Clear();
                }
            }

            float oldScale = -1f;

            //Создаёт отдельный рисунок для примера
            if (isCopyMode)
            {
                oldScale = Scale;
                Scale = 1;
            }

            Path mainPath = new Path();
            GeometryGroup geometryGroup = new GeometryGroup();
            GeometryGroup animatedGeometryGroup = new GeometryGroup();

            //Начало кнопок принятия и отклонения звонка

            PathGeometry pathGeometry = new PathGeometry();

            PathFigure acceptButton = new PathFigure(new Point(10 * Scale, 132 * Scale), new List<PathSegment>
            {
                new ArcSegment(new Point(23 * Scale, 132 * Scale), new Size(10, 9), 0, false, SweepDirection.Clockwise, true),
            }, false);

            PathFigure refuseButton = new PathFigure(new Point(77 * Scale, 132 * Scale), new List<PathSegment>
            {
                new ArcSegment(new Point(90 * Scale, 132 * Scale), new Size(10, 9), 0, false, SweepDirection.Clockwise, true),
            }, false);

            PathFigure figure = new PathFigure(new Point(0 * Scale, 132 * Scale), new List<PathSegment>
            {
                new ArcSegment(new Point(99 * Scale, 132 * Scale), new Size(30, 9), 0, false, SweepDirection.Counterclockwise, true),
            }, false);

            pathGeometry.Figures.Add(acceptButton);
            pathGeometry.Figures.Add(refuseButton);
            pathGeometry.Figures.Add(figure);
            geometryGroup.Children.Add(pathGeometry);

            //Конец кнопок принятия и отклонения звонка

            //Начало цыфер и доп знаков

            //1
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(15 * Scale, 152 * Scale), new Point(15 * Scale, 163 * Scale)));

            //2
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 152 * Scale), new Point(55 * Scale, 152 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 152 * Scale), new Point(55 * Scale, 157 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 157 * Scale), new Point(55 * Scale, 157 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 157 * Scale), new Point(45 * Scale, 163 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 163 * Scale), new Point(55 * Scale, 163 * Scale)));

            //3
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 152 * Scale), new Point(90 * Scale, 152 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 152 * Scale), new Point(90 * Scale, 157 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 157 * Scale), new Point(90 * Scale, 157 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 157 * Scale), new Point(90 * Scale, 163 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 163 * Scale), new Point(90 * Scale, 163 * Scale)));

            //4
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 172 * Scale), new Point(20 * Scale, 172 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 167 * Scale), new Point(10 * Scale, 172 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(20 * Scale, 167 * Scale), new Point(20 * Scale, 177 * Scale)));

            //5
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 167 * Scale), new Point(55 * Scale, 167 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 167 * Scale), new Point(45 * Scale, 172 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 172 * Scale), new Point(55 * Scale, 172 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 172 * Scale), new Point(55 * Scale, 177 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 177 * Scale), new Point(55 * Scale, 177 * Scale)));

            //6
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 167 * Scale), new Point(90 * Scale, 167 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 167 * Scale), new Point(78 * Scale, 177 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 172 * Scale), new Point(90 * Scale, 172 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 172 * Scale), new Point(90 * Scale, 177 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 177 * Scale), new Point(90 * Scale, 177 * Scale)));

            //7
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 182 * Scale), new Point(20 * Scale, 182 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(20 * Scale, 192 * Scale), new Point(20 * Scale, 182 * Scale)));

            //8
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 182 * Scale), new Point(55 * Scale, 182 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 182 * Scale), new Point(45 * Scale, 192 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 187 * Scale), new Point(55 * Scale, 187 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 182 * Scale), new Point(55 * Scale, 192 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 192 * Scale), new Point(55 * Scale, 192 * Scale)));

            //9
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 182 * Scale), new Point(90 * Scale, 182 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 182 * Scale), new Point(78 * Scale, 187 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 187 * Scale), new Point(90 * Scale, 187 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(90 * Scale, 182 * Scale), new Point(90 * Scale, 192 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(78 * Scale, 192 * Scale), new Point(90 * Scale, 192 * Scale)));

            //0
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 197 * Scale), new Point(55 * Scale, 197 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 197 * Scale), new Point(45 * Scale, 207 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(55 * Scale, 197 * Scale), new Point(55 * Scale, 207 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(45 * Scale, 207 * Scale), new Point(55 * Scale, 207 * Scale)));

            //*
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 197 * Scale), new Point(20 * Scale, 207 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(15 * Scale, 197 * Scale), new Point(15 * Scale, 207 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 202 * Scale), new Point(20 * Scale, 202 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 207 * Scale), new Point(20 * Scale, 197 * Scale)));

            //#
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(76 * Scale, 200 * Scale), new Point(91 * Scale, 200 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(80 * Scale, 207 * Scale), new Point(78 * Scale, 207 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(75 * Scale, 205 * Scale), new Point(90 * Scale, 205 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(86 * Scale, 207 * Scale), new Point(88 * Scale, 197 * Scale)));

            //Окончание цыфер и доп знаков

            //Просто кружки))
            geometryGroup.Children.Add(new EllipseGeometry(new Point(84 * Scale, 133 * Scale), 2 * Scale, 2 * Scale));
            geometryGroup.Children.Add(new EllipseGeometry(new Point(50 * Scale, 130 * Scale), 10 * Scale, 10 * Scale));

            //Линии на кнопках
            geometryGroup.Children.Add(new LineGeometry(new Point(10 * Scale, 117 * Scale), new Point(23 * Scale, 117 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(77 * Scale, 117 * Scale), new Point(90 * Scale, 117 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(33 * Scale, 150 * Scale), new Point(33 * Scale, 210 * Scale)));
            animatedGeometryGroup.Children.Add(new LineGeometry(new Point(66 * Scale, 150 * Scale), new Point(66 * Scale, 210 * Scale)));

            geometryGroup.Children.Add(new LineGeometry(new Point(0 * Scale, 110 * Scale), new Point(99 * Scale, 110 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(0 * Scale, 125 * Scale), new Point(33 * Scale, 125 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(66 * Scale, 125 * Scale), new Point(99 * Scale, 125 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(33 * Scale, 110 * Scale), new Point(33 * Scale, 146 * Scale)));
            geometryGroup.Children.Add(new LineGeometry(new Point(66 * Scale, 110 * Scale), new Point(66 * Scale, 146 * Scale)));


            animatedGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 150 * Scale, 99 * Scale, 15 * Scale)));
            animatedGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 165 * Scale, 99 * Scale, 15 * Scale)));
            animatedGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 180 * Scale, 99 * Scale, 15 * Scale)));
            animatedGeometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 195 * Scale, 99 * Scale, 15 * Scale)));

            //Рамка телефона
            geometryGroup.Children.Add(new RectangleGeometry(new Rect(0 * Scale, 0 * Scale, 99 * Scale, 150 * Scale)));

            mainPath.Data = geometryGroup;
            mainPath.Width = Width * Scale;
            mainPath.Height = Height * Scale;
            mainPath.Stroke = new SolidColorBrush(PenColor);
            mainPath.StrokeThickness = 2;

            Path animatedPath = new Path();
            animatedPath.Data = animatedGeometryGroup;
            animatedPath.Width = Width * Scale;
            animatedPath.Height = Height * Scale;
            animatedPath.Stroke = new SolidColorBrush(PenColor);
            animatedPath.StrokeThickness = 2;

            //Поле для вывода символов
            TextBlock textBlock = new TextBlock();
            textBlock.Width = Width * Scale;
            textBlock.Height = Height * Scale * 0.38;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.FontSize *= Scale;

            //Привязка к текста к полю с текстом 
            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath("Text");
            binding.Mode = BindingMode.TwoWay;
            textBlock.SetBinding(TextBlock.TextProperty, binding);

            if (isCopyMode)
            {
                animatedOriginalPhonePiece.Children.Add(animatedPath);
                originalPhone.Children.Add(animatedOriginalPhonePiece);

                Button hideButton = new Button();
                hideButton.Width = 121;
                hideButton.Content = "Спрятать/Показать";
                hideButton.Height = 20;
                hideButton.Click += (obj, arg) =>
                {
                    Animation();
                };
                Canvas.SetLeft(hideButton, -10);
                Canvas.SetTop(hideButton, 215);

                Button[] buttons = new Button[]
                {
                    new Button(), new Button(), new Button(), new Button(), new Button(), new Button(),
                    new Button(), new Button(), new Button(), new Button(), new Button(), new Button()
                };

                originalPhone.Children.Add(mainPath);

                for (int i = 0, count = 0, top = 150; i < buttons.Length; i++, count++)
                {

                    if (count > 2)
                    {
                        count = 0;
                        top += 15;
                    }

                    buttons[i].Width = 33;
                    buttons[i].Height = 15;
                    buttons[i].Opacity = 0;
                    Canvas.SetLeft(buttons[i], count * 33);
                    Canvas.SetTop(buttons[i], top);


                    if (i < 9)
                    {
                        int ind = i + 1;
                        buttons[i].Click += (o, e) =>
                        {
                            Text += ind.ToString();
                        };
                    }
                    else if (i == 9)
                    {
                        buttons[i].Click += (o, e) =>
                        {
                            Text += "*";
                        };
                        buttons[i + 1].Click += (o, e) =>
                        {
                            Text += "0";
                        };
                        buttons[i + 2].Click += (o, e) =>
                        {
                            Text += "#";
                        };
                    }

                    animatedOriginalPhonePiece.Children.Add(buttons[i]);
                }

                Button deleteButton = new Button();
                Button deleteAllButton = new Button();

                deleteButton.Width = 33;
                deleteButton.Height = 15;
                Canvas.SetLeft(deleteButton, 66);
                Canvas.SetTop(deleteButton, 110);

                deleteAllButton.Width = 33;
                deleteAllButton.Height = 15;
                Canvas.SetLeft(deleteAllButton, 66);
                Canvas.SetTop(deleteAllButton, 125);
                deleteAllButton.Opacity = 0;
                deleteButton.Opacity = 0;

                deleteAllButton.Click += (o, e) =>
                {
                    Text = "";
                };

                deleteButton.Click += (o, e) =>
                {
                    if (Text.Length > 0)
                    {
                        Text = Text.Remove(Text.Length - 1);
                    }
                };

                Scale = oldScale;


                originalPhone.Children.Add(hideButton);
                originalPhone.Children.Add(deleteButton);
                originalPhone.Children.Add(deleteAllButton);
                originalPhone.Children.Add(textBlock);


                PaintField.Children.Add(originalPhone);

            }
            else
            {
                Button trigger = new Button();
                trigger.Width = Width * Scale;
                trigger.Height = Height * Scale;
                trigger.Opacity = 0;

                animatedPiece.Children.Add(animatedPath);

                phoneField.Children.Add(animatedPiece);
                phoneField.Children.Add(mainPath);
                phoneField.Children.Add(textBlock);
                phoneField.Children.Add(trigger);



                PaintField.Children.Add(phoneField);

                isDrawed = true;
            }

        }

        public override void Animation()
        {
            //Ищет контейнер, в котором будет применяться анимация
            Path path = null;
            Path originalPhonePath = null;
            foreach (var item in animatedPiece.Children)
            {
                if (item is Path)
                {
                    path = (Path)item;
                }
            }
            
            foreach (var item in animatedOriginalPhonePiece.Children)
            {
                if (item is Path)
                {
                    originalPhonePath = (Path)item;
                }
            }

            //Нужно для того, чтобы клавиши не наезжали друг на друга, а скрывались друг под другом
            InkCanvas background = new InkCanvas();
            background.Height = (Height * Scale) / 2.9;
            background.Width = Width * Scale;
            background.Background = new SolidColorBrush(Colors.White);
            background.EditingMode = InkCanvasEditingMode.None;
            Canvas.SetTop(background, ((Height * Scale) - 1.9 * ((Height * Scale) / 3)) );
            phoneField.Children.Insert(1, background);

            InkCanvas originalPhoneBackground = new InkCanvas();
            originalPhoneBackground.Height = Height / 2.9;
            originalPhoneBackground.Width = Width;
            originalPhoneBackground.Background = new SolidColorBrush(Colors.White);
            originalPhoneBackground.EditingMode = InkCanvasEditingMode.None;
            Canvas.SetTop(originalPhoneBackground, (Height - 1.9 * (Height / 3)));
            originalPhone.Children.Insert(1, originalPhoneBackground);

            if (isHidden && path != null)
            {
                isAnimating = true;
                animatedPiece.Visibility = Visibility.Visible;
                animatedOriginalPhonePiece.Visibility = Visibility.Visible;

                ThicknessAnimation animation = new ThicknessAnimation();
                animation.From = new Thickness(0, -65 * Scale, 0, 0);
                animation.To = new Thickness(0);
                animation.Duration = new Duration(new System.TimeSpan(0, 0, 5));
                animation.Completed += (o, e) =>
                {
                    isHidden = false;
                    isAnimating = false;
                    phoneField.Children.RemoveAt(1);
                };
                animatedPiece.BeginAnimation(Canvas.MarginProperty, animation);

                ThicknessAnimation originalPhoneAnimation = new ThicknessAnimation();
                originalPhoneAnimation.From = new Thickness(0, -65, 0, 0);
                originalPhoneAnimation.To = new Thickness(0);
                originalPhoneAnimation.Duration = new Duration(new System.TimeSpan(0, 0, 5));
                originalPhoneAnimation.Completed += (o, e) =>
                {
                    originalPhone.Children.RemoveAt(1);
                };
                animatedOriginalPhonePiece.BeginAnimation(Canvas.MarginProperty, originalPhoneAnimation);
            }
            else if (path != null)
            {
                isAnimating = true;

                ThicknessAnimation animation = new ThicknessAnimation();
                animation.From = animatedPiece.Margin;
                animation.To = new Thickness(0, -65 * Scale, 0, 0);
                animation.Duration = new Duration(new System.TimeSpan(0, 0, 5));
                animation.Completed += (o, e) =>
                {
                    animatedPiece.Visibility = Visibility.Hidden;
                    isHidden = true;
                    isAnimating = false;
                    phoneField.Children.RemoveAt(1);
                };
                animatedPiece.BeginAnimation(Canvas.MarginProperty, animation);

                ThicknessAnimation originalPhoneAnimation = new ThicknessAnimation();
                originalPhoneAnimation.From = animatedOriginalPhonePiece.Margin;
                originalPhoneAnimation.To = new Thickness(0, -65, 0, 0);
                originalPhoneAnimation.Duration = new Duration(new System.TimeSpan(0, 0, 5));
                originalPhoneAnimation.Completed += (o, e) =>
                {
                    animatedOriginalPhonePiece.Visibility = Visibility.Hidden;
                    originalPhone.Children.RemoveAt(1);
                };
                animatedOriginalPhonePiece.BeginAnimation(Canvas.MarginProperty, originalPhoneAnimation);
            }
        }
    }


}
