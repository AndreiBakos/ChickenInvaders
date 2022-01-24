using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChickenInvaders
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Rectangle> _itemsToRemove = new List<Rectangle>();
        private int _maxNrOfEnemies = 25;
        private int _remainingLives = 5;
        private int _bulletTimer;
        private int _bulletTimerLimit = 90;
        private int _level = 1;
        private int _score = 0;
        private bool _goLeft, _goRight = false;
        private double _enemyBulletSpeed = 6;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            if (MessageBox.Show("Ready to start?", "Welcome to Chicken Invaders", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                OnStart();
            else 
                Environment.Exit(0);
        }

        private void OnStart()
        {
            dispatcherTimer.Tick += GameEngine;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(20);
            dispatcherTimer.Start();
            
            var spaceShipSkin = new ImageBrush();
            spaceShipSkin.ImageSource = new BitmapImage(new Uri(@"..\..\images\SpaceShip10.png", UriKind.Relative));
            Lives.Content = $"Lives left: {_remainingLives.ToString()}";
            Level.Content = $"Level: {_level.ToString()}";
            Score.Content = $"Score: {_score.ToString()}";
            CreateEnemies(_level);
            SpaceShip.Fill = spaceShipSkin;
            SpaceShip.Margin = new Thickness(0);
        }

        private void GameEngine(object sender, EventArgs e)
        {
            var player = new Rect(Canvas.GetLeft(SpaceShip), Canvas.GetTop(SpaceShip), SpaceShip.Width, SpaceShip.Height);
            
            if(Canvas.GetLeft(SpaceShip) > 0 && _goLeft)
                Canvas.SetLeft(SpaceShip, Canvas.GetLeft(SpaceShip) - 10);
            
            if(Canvas.GetLeft(SpaceShip) <  this.Width - 125 && _goRight)
                Canvas.SetLeft(SpaceShip,Canvas.GetLeft(SpaceShip) + 10);

            _bulletTimer -= 3;
            if (_bulletTimer < 0)
            {
                var enemy = MyCanvas.Children.OfType<Rectangle>();
                Random random = new Random();
                
                var result = enemy.Where(t => (string) t.Tag == "Enemy").ElementAtOrDefault(random.Next(0, enemy.Count() + 1));
                
                while (result == null)
                {
                    result = enemy.Where(t => (string) t.Tag == "Enemy").ElementAtOrDefault(random.Next(0, enemy.Count() + 1));
                }
                
                CreateEnemyBullet(Canvas.GetLeft(result) + 20, Canvas.GetTop(result));
                _bulletTimer = _bulletTimerLimit;
            }
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if (x is Rectangle && (string) x.Tag == "spaceShipBullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) - 20);
                    var bullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (Canvas.GetTop(x) < 10)
                    {
                        _itemsToRemove.Add(x);
                    }

                    foreach (var y in MyCanvas.Children.OfType<Rectangle>())
                    {
                        if (y is Rectangle && (string) y.Tag == "Enemy")
                        {
                            Rect enemy = new Rect(Canvas.GetLeft(y), Canvas.GetTop(y), y.Width, y.Height);
                            if (bullet.IntersectsWith(enemy))
                            {
                                _itemsToRemove.Add(x);
                                _itemsToRemove.Add(y);
                                _maxNrOfEnemies -= 1;
                                _score += 150;
                                Score.Content = $"Score: {_score.ToString()}";
                            }
                        }
                    }
                }
                if (x is Rectangle && (string)x.Tag == "enemyBullet")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + _enemyBulletSpeed);
                    Rect enemyBullet = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), 10, 10);
                    
                    if (Canvas.GetTop(x) > Canvas.GetTop(SpaceShip) + 30)
                    {
                        _itemsToRemove.Add(x);
                    }
                    if (enemyBullet.IntersectsWith(player))
                    {
                        _itemsToRemove.Add(x);
                        _remainingLives -= 1;
                        if(_remainingLives == 0)
                        {
                            dispatcherTimer.Stop();
                            if (MessageBox.Show($"Your Score is: {_score.ToString()}", "You Lost :(",
                                    MessageBoxButton.OK) == MessageBoxResult.OK)
                                Environment.Exit(0);
                        }
                        Lives.Content = $"Lives left: {_remainingLives.ToString()}";
                    }
                }
            }
            foreach (var r in _itemsToRemove)
            {
                MyCanvas.Children.Remove(r);
            }
            if (_maxNrOfEnemies < 1)
            {
                _level += 1;
                _maxNrOfEnemies = 25;
                Level.Content = $"Level: {_level.ToString()}";
                _enemyBulletSpeed += .5;
                CreateEnemies(_level);
            }
        }

        private void Restart()
        {
            _maxNrOfEnemies = 25;
            _remainingLives = 5;
            _bulletTimerLimit = 90;
            _level = 1; 
            _goLeft = false; _goRight = false;
            _enemyBulletSpeed = 6;
            foreach (var x in MyCanvas.Children.OfType<Rectangle>())
            {
                if (x is Rectangle && (string) x.Tag == "enemyBullet")
                {
                    MyCanvas.Children.Remove(x);
                }
            }
            // CreateEnemies(_level);
            dispatcherTimer.Start();
        }
        private void CreateEnemies(int currentLevel)
        {
            var enemySkin1 = new ImageBrush();
            var enemySkin2 = new ImageBrush();
            
            enemySkin1.ImageSource = new BitmapImage(new Uri(@"..\..\images\Chicken7.png", UriKind.Relative));
            enemySkin2.ImageSource = new BitmapImage(new Uri(@"..\..\images\Chicken8.png", UriKind.Relative));
            
            const int left = 90;
            var top = 40;
            var helperForLeft = 0;
            
            var random = new Random();
            
            for (var i = 0; i < _maxNrOfEnemies; i++)
            {
                var chicken = random.Next(7, 9);
                Rectangle enemy = new Rectangle()
                {
                    Tag = "Enemy",
                    Width = 80,
                    Height = 80,
                    Fill = currentLevel == 1 ? enemySkin1 : chicken == 7 ? enemySkin1 : enemySkin2
                };

                if (i % 5 == 0 && i != 0)
                {
                    helperForLeft = 0;
                }

                Canvas.SetTop(enemy, i % 5 == 0 && i != 0 ? top += 80 : top);
                Canvas.SetLeft(enemy, left + (helperForLeft * 150));
                MyCanvas.Children.Add(enemy);

                helperForLeft++;
            }
        }

        private void CreateSpaceShipBullet()
        {
            var bullet = new ImageBrush();
            bullet.ImageSource = new BitmapImage(new Uri(@"..\..\images\Bullet1.png", UriKind.Relative));

            Rectangle spaceshipBullet = new Rectangle()
            {
                Tag = "spaceShipBullet",
                Width = 30,
                Height = 30,
                Fill = bullet
            };
            Canvas.SetTop(spaceshipBullet, Canvas.GetTop(SpaceShip) - 30);
            Canvas.SetLeft(spaceshipBullet, Canvas.GetLeft(SpaceShip) + 25);
            MyCanvas.Children.Add(spaceshipBullet);
            // dispatcherTimer.Stop();
        }

        private void CreateEnemyBullet(double x, double y)
        {
            var egg = new ImageBrush();
            var fireEgg = new ImageBrush();
            var enchantedEgg = new ImageBrush();
            
            egg.ImageSource = new BitmapImage(new Uri(@"..\..\images\Egg1.png", UriKind.Relative));
            fireEgg.ImageSource = new BitmapImage(new Uri(@"..\..\images\FireEgg.png", UriKind.Relative));
            enchantedEgg.ImageSource = new BitmapImage(new Uri(@"..\..\images\EnchantedEgg.png", UriKind.Relative));
            
            Rectangle enemyBullet = new Rectangle()
            {
                Tag = "enemyBullet",
                Width = 30,
                Height = 30,
                Fill = egg
            };
            Canvas.SetTop(enemyBullet, y);
            Canvas.SetLeft(enemyBullet, x);
            MyCanvas.Children.Add(enemyBullet);
        }
        private void SpaceShip_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            
                _goLeft = true;

            if (e.Key == Key.Right)
                _goRight = true;
        }

        private void SpaceShip_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
                _goLeft = false;
            
            if (e.Key == Key.Space)
            {
                CreateSpaceShipBullet();
            }
            if (e.Key == Key.Right)
                _goRight = false;
        }
    }
}
