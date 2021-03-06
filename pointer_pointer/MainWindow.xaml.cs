//------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// This module contains code to do Kinect NUI initialization,
// processing, displaying players on screen, and sending updated player
// positions to the game portion for hit testing.

namespace ShapeGame
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Controls;
    using System.Windows.Threading;
    using Microsoft.Kinect;
    using ShapeGame.Speech;
    using ShapeGame.Utils;

    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    using System.Resources;
    using System.Drawing;

    using Coding4Fun.Kinect.Wpf;
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        #region Private State
        private const int TimerResolution = 2;  // ms
        private const int NumIntraFrames = 3;
        private const int MaxShapes = 80;
        private const double MaxFramerate = 70;
        private const double MinFramerate = 15;
        private const double MinShapeSize = 12;
        private const double MaxShapeSize = 90;
        private const double DefaultDropRate = 2.5;
        private const double DefaultDropSize = 32.0;
        private const double DefaultDropGravity = 2;

        private readonly Dictionary<int, Player> players = new Dictionary<int, Player>();
        private readonly SoundPlayer popSound = new SoundPlayer();
        private readonly SoundPlayer hitSound = new SoundPlayer();
        private readonly SoundPlayer squeezeSound = new SoundPlayer();

        private readonly SoundPlayer openingSound = new SoundPlayer();
        private readonly SoundPlayer calibrateSound = new SoundPlayer();
        private readonly SoundPlayer stage1Sound = new SoundPlayer();
        private readonly SoundPlayer stage2Sound = new SoundPlayer();
        private readonly SoundPlayer stage3Sound = new SoundPlayer();
        private readonly SoundPlayer stage4Sound = new SoundPlayer();
        private readonly SoundPlayer successSound = new SoundPlayer();
        private readonly SoundPlayer goalSound = new SoundPlayer();
        private readonly SoundPlayer resultSound = new SoundPlayer();


        private double dropRate = DefaultDropRate;
        private double dropSize = DefaultDropSize;
        private double dropGravity = DefaultDropGravity;
        private DateTime lastFrameDrawn = DateTime.MinValue;
        private DateTime predNextFrame = DateTime.MinValue;
        private double actualFrameTime;

        private Skeleton[] skeletonData;

        // Player(s) placement in scene (z collapsed):
        private Rect playerBounds;
        private Rect screenRect;

        private double targetFramerate = MaxFramerate;
        private int frameCount;
        private bool runningGameThread;
        private FallingThings myFallingThings;
        private int playersAlive;


        private BoundingBoxes myBoundingBoxes;



        private const float SkeletonMaxX = 0.60f;
        private const float SkeletonMaxY = 0.40f;

        private int playerNum = 0;



        private Photos photos = new Photos();


        #endregion Private State



        #region ctor + Window Events

        public MainWindow()
        {
            InitializeComponent();
            this.RestoreWindowState();
            this.KeyDown += new KeyEventHandler(OnButtonKeyDown);
        }

        private void OnButtonKeyDown(object sender, KeyEventArgs e)
        {
        }
        // Since the timer resolution defaults to about 10ms precisely, we need to
        // increase the resolution to get framerates above between 50fps with any
        // consistency.
        [DllImport("Winmm.dll", EntryPoint = "timeBeginPeriod")]
        private static extern int TimeBeginPeriod(uint period);

        private void RestoreWindowState()
        {
            // Restore window state to that last used
            Rect bounds = Properties.Settings.Default.PrevWinPosition;
            if (bounds.Right != bounds.Left)
            {
                this.Top = bounds.Top;
                this.Left = bounds.Left;
                this.Height = bounds.Height;
                this.Width = bounds.Width;
            }

            this.WindowState = (WindowState)Properties.Settings.Default.WindowState;
        }

        private void WindowLoaded(object sender, EventArgs e)
        {
            playfield.ClipToBounds = true;

            this.myFallingThings = new FallingThings(MaxShapes, this.targetFramerate, NumIntraFrames);
            this.myBoundingBoxes = new BoundingBoxes();

            this.UpdatePlayfieldSize();

            this.myFallingThings.SetGravity(this.dropGravity);
            this.myFallingThings.SetDropRate(this.dropRate);
            this.myFallingThings.SetSize(this.dropSize);
            this.myFallingThings.SetPolies(PolyType.All);
            this.myFallingThings.SetGameMode(GameMode.Off);

            SensorChooser.KinectSensorChanged += this.SensorChooserKinectSensorChanged;

            this.popSound.Stream = Properties.Resources.Pop_5;
            this.hitSound.Stream = Properties.Resources.Hit_2;
            this.squeezeSound.Stream = Properties.Resources.Squeeze;

            this.openingSound.Stream = Properties.Resources.opening;
            this.calibrateSound.Stream = Properties.Resources.calibrate_loop;
            this.stage1Sound.Stream = Properties.Resources.stage1;
            this.stage2Sound.Stream = Properties.Resources.stage2;
            this.stage3Sound.Stream = Properties.Resources.stage3;
            this.stage4Sound.Stream = Properties.Resources.stage4;
            this.successSound.Stream = Properties.Resources.success;
            this.goalSound.Stream = Properties.Resources.goal;
            this.resultSound.Stream = Properties.Resources.result_scene;

            //this.popSound.Play();

            ImageLoader();

            TimeBeginPeriod(TimerResolution);

            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dispatcherTimer.Start();

            System.Windows.Threading.DispatcherTimer dispatcherTimer2 = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer2.Tick += new EventHandler(dispatcherTimer_MoveChecker);
            dispatcherTimer2.Interval = new TimeSpan(0, 0, 0, 0, 1000);
            dispatcherTimer2.Start();

            var myGameThread = new Thread(this.GameThread);
            myGameThread.SetApartmentState(ApartmentState.STA);
            myGameThread.Start();

            FlyingText.NewFlyingText(this.screenRect.Width / 30, new System.Windows.Point(this.screenRect.Width / 2, this.screenRect.Height / 2), " ");


        }

        private void WindowClosing(object sender, CancelEventArgs e)
        {
            this.runningGameThread = false;
            Properties.Settings.Default.PrevWinPosition = this.RestoreBounds;
            Properties.Settings.Default.WindowState = (int)this.WindowState;
            Properties.Settings.Default.Save();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            SensorChooser.Kinect = null;
        }

        #endregion ctor + Window Events

        #region Kinect discovery + setup

        private void SensorChooserKinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                this.UninitializeKinectServices((KinectSensor)e.OldValue);
            }


            if (e.NewValue != null)
            {
                this.InitializeKinectServices((KinectSensor)e.NewValue);
            }
        }

        // Kinect enabled apps should customize which Kinect services it initializes here.
        private KinectSensor InitializeKinectServices(KinectSensor sensor)
        {
            // Application should enable all streams first.
            sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);



            sensor.SkeletonFrameReady += this.SkeletonsReady;
            sensor.SkeletonStream.Enable(new TransformSmoothParameters()
                                             {
                                                 Smoothing = 0.5f,
                                                 Correction = 0.5f,
                                                 Prediction = 0.5f,
                                                 JitterRadius = 0.05f,
                                                 MaxDeviationRadius = 0.04f
                                             });

            try
            {
                sensor.Start();
            }
            catch (IOException)
            {
                SensorChooser.AppConflictOccurred();
                return null;
            }


            


            return sensor;
        }

        // Kinect enabled apps should uninitialize all Kinect services that were initialized in InitializeKinectServices() here.
        private void UninitializeKinectServices(KinectSensor sensor)
        {
            sensor.Stop();

            sensor.SkeletonFrameReady -= this.SkeletonsReady;

        }

        #endregion Kinect discovery + setup

        #region Kinect Skeleton processing




        private void SkeletonsReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            playerNum = 0;
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    int skeletonSlot = 0;

                    if ((this.skeletonData == null) || (this.skeletonData.Length != skeletonFrame.SkeletonArrayLength))
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    }

                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);

                    foreach (Skeleton skeleton in this.skeletonData)
                    {
                        if (SkeletonTrackingState.Tracked == skeleton.TrackingState)
                        {
                            Player player;
                            if (this.players.ContainsKey(skeletonSlot))
                            {
                                player = this.players[skeletonSlot];
                            }
                            else
                            {
                                player = new Player(skeletonSlot);
                                player.SetBounds(this.playerBounds);
                                this.players.Add(skeletonSlot, player);
                            }

                            player.LastUpdated = DateTime.Now;

                            ++playerNum;

                            // Update player's bone and joint positions
                            if (skeleton.Joints.Count > 0)
                            {
                                player.IsAlive = true;
                                
                                // Head, hands, feet (hit testing happens in order here)
                                player.UpdateJointPosition(skeleton.Joints, JointType.Head);
                                player.UpdateJointPosition(skeleton.Joints, JointType.HandLeft);
                                player.UpdateJointPosition(skeleton.Joints, JointType.HandRight);
                                player.UpdateJointPosition(skeleton.Joints, JointType.FootLeft);
                                player.UpdateJointPosition(skeleton.Joints, JointType.FootRight);

                                // Hands and arms
                                player.UpdateBonePosition(skeleton.Joints, JointType.HandRight, JointType.WristRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.WristRight, JointType.ElbowRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ElbowRight, JointType.ShoulderRight);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HandLeft, JointType.WristLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.WristLeft, JointType.ElbowLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ElbowLeft, JointType.ShoulderLeft);

                                // Head and Shoulders
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderCenter, JointType.Head);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderLeft, JointType.ShoulderCenter);
                                player.UpdateBonePosition(skeleton.Joints, JointType.ShoulderCenter, JointType.ShoulderRight);

                                // Legs
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipLeft, JointType.KneeLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.KneeLeft, JointType.AnkleLeft);
                                player.UpdateBonePosition(skeleton.Joints, JointType.AnkleLeft, JointType.FootLeft);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HipRight, JointType.KneeRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.KneeRight, JointType.AnkleRight);
                                player.UpdateBonePosition(skeleton.Joints, JointType.AnkleRight, JointType.FootRight);

                                player.UpdateBonePosition(skeleton.Joints, JointType.HipLeft, JointType.HipCenter);
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipCenter, JointType.HipRight);

                                // Spine
                                player.UpdateBonePosition(skeleton.Joints, JointType.HipCenter, JointType.ShoulderCenter);

                                

                                // for cursor
                                var wristRight = skeleton.Joints[JointType.WristRight];
                                var scaledRightHand = wristRight.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, SkeletonMaxX, SkeletonMaxY);
                                //var scaledRightHand = wristRight;


                                //var cursorX = scaledRightHand.Position.X;
                                var cursorY = scaledRightHand.Position.Y;

                                var cursorX = Math.Abs(1024 - scaledRightHand.Position.X);
                                //var cursorY = Math.Abs(768 - scaledRightHand.Position.Y);
                                /*
                                var seg = new Segment(
    (joints[j1].Position.X * this.playerScale) + this.playerCenter.X,
    this.playerCenter.Y - (joints[j1].Position.Y * this.playerScale),
    (joints[j2].Position.X * this.playerScale) + this.playerCenter.X,
    this.playerCenter.Y - (joints[j2].Position.Y * this.playerScale)) { Radius = Math.Max(3.0, this.playerBounds.Height * BoneSize) / 2 };
                                this.UpdateSegmentPosition(j1, j2, seg);
                                
                                double playerScale = player.GetPlayerScale();
                                double playerCenterX = player.GetPlayerCenterX();
                                double playerCenterY = player.GetPlayerCenterY();

                                var cursorX = (wristRight.Position.X * playerScale) + playerCenterX;
                                var cursorY = playerCenterY - (wristRight.Position.Y * playerScale);
                                */

                                nowMovedX = (int)cursorX;
                                nowMovedY = (int)cursorY;
                                Console.WriteLine("X:" + cursorX + " Y:" + cursorY);

                                
                            }
                        }

                        skeletonSlot++;
                    }
                }
            }
        }

        private void CheckPlayers()
        {
            foreach (var player in this.players)
            {
                if (!player.Value.IsAlive)
                {
                    // Player left scene since we aren't tracking it anymore, so remove from dictionary
                    this.players.Remove(player.Value.GetId());
                    break;
                }
            }

            // Count alive players
            int alive = this.players.Count(player => player.Value.IsAlive);

            if (alive != this.playersAlive)
            {
                if (alive == 2)
                {
                    this.myFallingThings.SetGameMode(GameMode.TwoPlayer);
                }
                else if (alive == 1)
                {
                    this.myFallingThings.SetGameMode(GameMode.Solo);
                }
                else if (alive == 0)
                {
                    this.myFallingThings.SetGameMode(GameMode.Off);
                }

                if ((this.playersAlive == 0) )
                {
                    BannerText.NewBanner(
                        Properties.Resources.Vocabulary,
                        this.screenRect,
                        true,
                        System.Windows.Media.Color.FromArgb(200, 255, 255, 255));
                }

                this.playersAlive = alive;
            }
        }

        private void PlayfieldSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdatePlayfieldSize();
        }

        private void UpdatePlayfieldSize()
        {
            // Size of player wrt size of playfield, putting ourselves low on the screen.
            this.screenRect.X = 0;
            this.screenRect.Y = 0;
            this.screenRect.Width = this.playfield.ActualWidth;
            this.screenRect.Height = this.playfield.ActualHeight;

            BannerText.UpdateBounds(this.screenRect);

            this.playerBounds.X = 0;
            this.playerBounds.Width = this.playfield.ActualWidth;
            this.playerBounds.Y = this.playfield.ActualHeight * 0.2;
            this.playerBounds.Height = this.playfield.ActualHeight * 0.75;

            foreach (var player in this.players)
            {
                player.Value.SetBounds(this.playerBounds);
            }

            Rect fallingBounds = this.playerBounds;
            fallingBounds.Y = 0;
            fallingBounds.Height = playfield.ActualHeight;
            if (this.myFallingThings != null)
            {
                this.myFallingThings.SetBoundaries(fallingBounds);
            }
        }
        #endregion Kinect Skeleton processing

        #region GameTimer/Thread

        bool sceneTrigger = false;
        enum GameScene {
            GAME_SCENE_NONE,
            GAME_SCENE_INTRO_0_0_TITLE,
            GAME_SCENE_INTRO_0_1_RULE,
            GAME_SCENE_STAGE_1_0_STAGE_LEFT,
            GAME_SCENE_STAGE_1_1_CALIBRATE,
            GAME_SCENE_STAGE_1_2_MAP_LOAD,
            GAME_SCENE_STAGE_1_3_GAME_PLAY,
            GAME_SCENE_STAGE_1_4_RESULT,
            GAME_SCENE_STAGE_2_0_STAGE_LEFT,
            GAME_SCENE_STAGE_2_1_CALIBRATE,
            GAME_SCENE_STAGE_2_2_MAP_LOAD,
            GAME_SCENE_STAGE_2_3_GAME_PLAY,
            GAME_SCENE_STAGE_2_4_RESULT,
            GAME_SCENE_STAGE_3_0_STAGE_LEFT,
            GAME_SCENE_STAGE_3_1_CALIBRATE,
            GAME_SCENE_STAGE_3_2_MAP_LOAD,
            GAME_SCENE_STAGE_3_3_GAME_PLAY,
            GAME_SCENE_STAGE_3_4_RESULT,
            GAME_SCENE_STAGE_4_0_STAGE_LEFT,
            GAME_SCENE_STAGE_4_1_CALIBRATE,
            GAME_SCENE_STAGE_4_2_MAP_LOAD,
            GAME_SCENE_STAGE_4_3_GAME_PLAY,
            GAME_SCENE_STAGE_4_4_RESULT,
            GAME_SCENE_RESULT_5_0_GOAL,
            GAME_SCENE_RESULT_5_1_TITLE,
            GAME_SCENE_RESULT_5_2_MOVIE,
            GAME_SCENE_RESULT_5_3_END,
            GAME_SCENE_MAX 
        };

        private GameScene gameScene = GameScene.GAME_SCENE_NONE;
        private bool shutDownAPP = false;
        private void GameThread()
        {
            this.runningGameThread = true;
            this.predNextFrame = DateTime.Now;
            this.actualFrameTime = 1000.0 / this.targetFramerate;
            
            this.gameScene = GameScene.GAME_SCENE_INTRO_0_0_TITLE;
            //this.gameScene = GameScene.GAME_SCENE_STAGE_4_0_STAGE_LEFT;

            timerOn = true;
            timerTick = 0;
            timerMax = 40;
            sceneTrigger = false;
            openingSound.Play();

            myBoundingBoxes.SetBounds(this.playerBounds);
            //myBoundingBoxes.AddBox(200, 250, 300, 350);
            //myBoundingBoxes.AddBox(400, 250, 500, 300);


            // Try to dispatch at as constant of a framerate as possible by sleeping just enough since
            // the last time we dispatched.
            while (this.runningGameThread)
            {
                if (shutDownAPP)
                {
                    break;
                }
                
                SceneCheck();
                //Console.WriteLine("Num:" + this.myFallingThings.GetThingsNum());


                // Calculate average framerate.  
                DateTime now = DateTime.Now;
                if (this.lastFrameDrawn == DateTime.MinValue)
                {
                    this.lastFrameDrawn = now;
                }

                double ms = now.Subtract(this.lastFrameDrawn).TotalMilliseconds;
                this.actualFrameTime = (this.actualFrameTime * 0.95) + (0.05 * ms);
                this.lastFrameDrawn = now;

                // Adjust target framerate down if we're not achieving that rate
                this.frameCount++;
                if ((this.frameCount % 100 == 0) && (1000.0 / this.actualFrameTime < this.targetFramerate * 0.92))
                {
                    this.targetFramerate = Math.Max(MinFramerate, (this.targetFramerate + (1000.0 / this.actualFrameTime)) / 2);
                }

                if (now > this.predNextFrame)
                {
                    this.predNextFrame = now;
                }
                else
                {
                    double milliseconds = this.predNextFrame.Subtract(now).TotalMilliseconds;
                    if (milliseconds >= TimerResolution)
                    {
                        Thread.Sleep((int)(milliseconds + 0.5));
                    }
                }

                this.predNextFrame += TimeSpan.FromMilliseconds(1000.0 / this.targetFramerate);

                if (null != this.skeletonData && 0 < this.skeletonData.Length)
                {
                    foreach (Skeleton skeleton in this.skeletonData)
                    {
                        try
                        {
                            if (SkeletonTrackingState.Tracked == skeleton.TrackingState)
                            {
                                //Joint joint = skeleton.Joints[JointType.HandLeft];
                                myBoundingBoxes.IsBounced(skeleton);
                                //Console.WriteLine("1:" + joint.Position.X);
                            }
                        }
                        catch (Exception)
                        {
                        }
                        
                    }
                }
                 
                /*
                foreach (int skeletonSlot in this.players.Keys)
                {
                    if (this.players.ContainsKey(skeletonSlot))
                    {
                        Player player = this.players[skeletonSlot];
                        Dictionary<Bone, BoneData> segments = player.Segments;
                        DateTime cur = DateTime.Now;
                        try
                        {


                            foreach (var segment in segments)
                            {
                                Segment seg = segment.Value.GetEstimatedSegment(cur);
                                if (seg.IsCircle())
                                {
                                    Console.WriteLine("1:" + seg.X1);
                                }
                                skeleton.Joints[seg];
                                
                            }


                        }
                        catch
                        {

                        }
                        
                    }
                }
                */
                

                this.Dispatcher.Invoke(DispatcherPriority.Send, new Action<int>(this.HandleGameTimer), 0);
            }


        }

        private void HandleGameTimer(int param)
        {

            // Every so often, notify what our actual framerate is
            if ((this.frameCount % 100) == 0)
            {
                this.myFallingThings.SetFramerate(1000.0 / this.actualFrameTime);
            }

            // Advance animations, and do hit testing.
            for (int i = 0; i < NumIntraFrames; ++i)
            {
                foreach (var pair in this.players)
                {
                    HitType hit = this.myFallingThings.LookForHits(pair.Value.Segments, pair.Value.GetId());
                    if ((hit & HitType.Squeezed) != 0)
                    {
                        //this.squeezeSound.Play();
                    }
                    else if ((hit & HitType.Popped) != 0)
                    {
                        //this.popSound.Play();
                    }
                    else if ((hit & HitType.Hand) != 0)
                    {
                        //this.hitSound.Play();
                    }
                }

                this.myFallingThings.AdvanceFrame();

            }

            // Draw new Wpf scene by adding all objects to canvas
            playfield.Children.Clear();
            this.myFallingThings.DrawFrame(this.playfield.Children);
            foreach (var player in this.players)
            {
                player.Value.Draw(playfield.Children);
            }

            ImageChanger();
            myBoundingBoxes.DrawBoxes(playfield.Children);
            

            BannerText.Draw(playfield.Children);
            FlyingText.Draw(playfield.Children);

            this.CheckPlayers();
        }


        private ResourceManager rm = new ResourceManager("ShapeGame.Properties.Resources", System.Reflection.Assembly.GetExecutingAssembly());

        private BitmapImage IMAGE_IDLE = new BitmapImage();
        private BitmapImage IMAGE_POINTER;

        private void ImageLoader()
        {
            ImageLoad(IMAGE_IDLE, "calibrate");

        }
        private void ImageLoad(BitmapImage bitmapImage, String name)
        {


            Bitmap bitmapimg = (Bitmap)rm.GetObject(name); //예를들어 Myimg.png를 읽어 리소스 매니저에서Myimg1이 되었다면 Myimg1을 적어야 한다.

            System.Drawing.Image img = (System.Drawing.Image)bitmapimg;
            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            bitmapImage.BeginInit();
            bitmapImage.StreamSource = ms;
            bitmapImage.EndInit();
        }

       
        private void ImageChanger()
        {
            if (moveCheckOn)
            {
                UI_Layer1.Source = IMAGE_IDLE;
            }
            else
            {
                UI_Layer1.Source = IMAGE_POINTER;
            }
            
        }
        
        private void StopAllSound()
        {
            openingSound.Stop();
            calibrateSound.Stop();
            stage1Sound.Stop();
            stage2Sound.Stop();
            stage3Sound.Stop();
            stage4Sound.Stop();
            successSound.Stop();
            goalSound.Stop();
            resultSound.Stop();
        }

        private void SceneCheck()
        {
            switch (this.gameScene)
            {
                case GameScene.GAME_SCENE_INTRO_0_0_TITLE:
                    if (sceneTrigger)
                    {
                        this.gameScene = GameScene.GAME_SCENE_INTRO_0_1_RULE;
                        timerTick = 0;
                        timerMax = 30;
                        timerOn = true;
                        sceneTrigger = false;
                    }
                    break;
                case GameScene.GAME_SCENE_INTRO_0_1_RULE:
                    if (sceneTrigger)
                    {
                        this.gameScene = GameScene.GAME_SCENE_STAGE_1_1_CALIBRATE;
                        timerTick = 0;
                        timerMax = 15;
                        timerOn = true;
                        sceneTrigger = false;
                        StopAllSound();
                        calibrateSound.Play();
                    }
                    break;
                case GameScene.GAME_SCENE_STAGE_1_1_CALIBRATE:
                    break;
            }

        }


        private bool timerOn = false;
        private uint timerTick = 0;
        private uint timerMax = 0;
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (timerOn)
            {
                ++timerTick;

                if (timerTick > timerMax)
                {
                    sceneTrigger = true;
                    timerOn = false;
                    timerTick = 0;
                    moveCheckOn = true;
                }
            }

            if (shutDownAPP)
            {
                Application.Current.Shutdown();
            }
        }


        private bool moveCheckOn = true;
        private int lastMovedX = 9999;
        private int lastMovedY = 9999;
        private int nowMovedX = 9999;
        private int nowMovedY = 9999;
        private void dispatcherTimer_MoveChecker(object sender, EventArgs e)
        {
            if(moveCheckOn)
            {
                if(playerNum==0)
                {
                    return;
                }

                int lastPosition = lastMovedX + lastMovedY;
                int nowPosition = nowMovedX + nowMovedY;

                if ( nowPosition > 10000 )
                {
                    return;
                }

                int difference = Math.Abs(lastPosition - nowPosition);

                if (difference > 20)
                {
                    lastMovedX = nowMovedX;
                    lastMovedY = nowMovedY;
                }
                else
                {
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ViewPhoto");
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ViewPhoto");
                    Console.WriteLine("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!ViewPhoto");


                    PhotoBox photoBox = photos.GetNearestPhotoBox(nowMovedX, nowMovedY);
                    IMAGE_POINTER = photoBox.GetPhoto();
                    moveCheckOn = false;
                    timerTick = 0;
                    timerMax = 35;
                    timerOn = true;

                    lastMovedX = 9999;
                    lastMovedY = 9999;
                }
            }


            if (shutDownAPP)
            {
                Application.Current.Shutdown();
            }
        }
        #endregion GameTimer/Thread




    }
}
