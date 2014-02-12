/*
 * Miguel Ángel Muñoz Ferreira
 * Mario Heredia Moreno
 * 
 * Nuevos Paradigmas de Interacción
 * 
 * Universidad de Granada
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Ink;
using Microsoft.Kinect;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;
using System.IO;
using System.Windows.Forms;
using Coding4Fun.Kinect.Wpf;
using Coding4Fun.Kinect.Wpf.Controls;



namespace PaintProyect
{
	/// <summary>
	/// Lógica de interacción para MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/**********************************************************************************************/
		//Instancia del sensor
		private KinectSensor sensor;

        //Instancia del reconocedor de habla
        private SpeechRecognitionEngine speechEngine;

        //Booleano de idioma diccionario
        private bool recognizerES;

        //Posicion inicial de la flecha que indica el color del pincel
        private const int posYArrow = 340;

		//Variables necesarias para el dibujo
		private System.Drawing.Point[] points;
		private System.Drawing.Pen pen;
		private System.Drawing.Bitmap bitmap;
		private string action;
		private bool drawing;
		
		//Vector de esqueletos reconocidos
		private const int skeletonCount = 6;
		private Skeleton[] allSkeleton = new Skeleton[skeletonCount];
		
		//Variables necesarias para reconocer cuándo dos objetos están cerca para poder
 		//interaccionar
		private static double _itemTop;
		private static double _topBoundary;
		private static double _bottomBoundary;
		private static double _itemLeft;
		private static double _leftBoundary;
		private static double _rightBoundary;
		
		
		/**********************************************************************************************/

		public MainWindow()
		{
			InitializeComponent();
		}

		/**********************************************************************************************/
		//Ejecución de tareas de inicialización
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			
			//Inicializamos el sensor
			foreach (var potentialSensor in KinectSensor.KinectSensors)
			{
				if (potentialSensor.Status == KinectStatus.Connected)
				{
					this.sensor = potentialSensor;
					break;
				}
			}

			//Si el sensor está conectado
			if (null != this.sensor)
			{
				//Parámetros con los que vamos a habilitar el SkeltonStream, con ellos
				//suavizamos los gestos que capta el sensor
				var parameters = new TransformSmoothParameters
                {
					Correction = 0.4f,
					JitterRadius = 0.05f,
					MaxDeviationRadius = 0.04f, 
					Prediction = 0.4f,
					Smoothing = 0.5f
				};
				
				//Activamos skeletonStream con parámetros
				this.sensor.SkeletonStream.Enable(parameters);
				this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
				this.sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

				//Añadimos un manejador de eventos que se llamará cada vez que se actualicen los frames
				this.sensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);

				//Comprobará si se hace click sobre la imagen
				hoverSave.Click += new RoutedEventHandler(hoverSave_Click);
				hoverExit.Click += new RoutedEventHandler(hoverExit_Click);

				//Comprobará el uso que se hace del ratón (las manos): botón izquierdo presionado, no presionado y movimiento
				this.paintPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.paintPanel_MouseDown);
				this.paintPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.paintPanel_MouseMove);
				this.paintPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.paintPanel_MouseUp);

				//Inicializamos variables para el dibujo
				action = "dibujar";
				points = new System.Drawing.Point[0];
				pen = new System.Drawing.Pen(System.Drawing.Color.Black, 5);
				bitmap = new System.Drawing.Bitmap(paintPanel.Width, paintPanel.Height);			
				
				try
				{
					//Comienza a capturar imágenes
					this.sensor.Start();
				}
				catch (IOException)
				{
					this.sensor = null;
				}
			}

            //Variable que intenta encontrar un languaje pack válido haciendo uso de GetKinectRecognizer()
            RecognizerInfo ri = GetKinectRecognizer();

			//Si tenemos instalado el paquete de idiomas de español nos aparece la bandera española
			if (recognizerES)
			{
				imageSpain.Visibility = System.Windows.Visibility.Visible;
			}
			//En caso contrario, nos aparece la de EEUU
			else 
			{
				imageEEUU.Visibility = System.Windows.Visibility.Visible;
			}
			
			//Si se ha encontrado un reconocedor válido
            if (null != ri)
            {
                //Asignamos el languaje pack a nuestro reconocedor
                this.speechEngine = new SpeechRecognitionEngine(ri.Id);

                //Creamos la grámatica desde el fichero XML
                Grammar g = CreateGrammarFromStream();

                //La cargamos
                speechEngine.LoadGrammar(g);

                //Inicializamos nuestro reconocedor y llamamos a speechEngine_SpeechRecognized cada vez que detectemos una palabra
                speechEngine.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(speechEngine_SpeechRecognized);
                speechEngine.SetInputToAudioStream(sensor.AudioSource.Start(), new SpeechAudioFormatInfo(EncodingFormat.Pcm, 16000, 16, 1, 32000, 2, null));
                speechEngine.RecognizeAsync(RecognizeMode.Multiple);
            } 

		}

		/**********************************************************************************************/
		//Salida de la aplicación mediante el click sobre la imagen
		void hoverExit_Click(object sender, RoutedEventArgs e)
		{
			sensorOff();
            speechOff();
			System.Environment.Exit(0);
		}

		/**********************************************************************************************/
		//Guardamos la imagen pintada mediante el click sobre la imagen
		void hoverSave_Click(object sender, RoutedEventArgs e)
		{
			saveImage();
		}

		/**********************************************************************************************/
		//Tareas a realizar cuando el dispositivo reconozca un cambio en la imagen
		void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
		{
			Skeleton first = GetFirstSkeleton(e);
			if (first == null)
			{
				return;
			}

			//Captamos la posición del skeleton para mover el ratón
			GetCameraPoint(first, e);

			//Si no se está dibujando, hacemos que aparezca la mano para seleccionar objetos
			if(!drawing)
				ScalePosition(imageHand, first.Joints[JointType.HandRight]);

			//Comprobamos si la mano está sobre las imágenes de guardar o salir
			PushButton(hoverSave, imageHand);
			PushButton(hoverExit, imageHand);

			//Indicamos al usuario si puede pintar o no, dependiendo de dónde se sitúe su mano izquierda respecto a su cintura
			if (first.Joints[JointType.HandLeft].Position.Y > first.Joints[JointType.HipCenter].Position.Y)
			{
				textBlock1.Text = "Ahora puedes pintar!!";
				drawing = true;
			}
			else
			{
				textBlock1.Text = "No puedes pintar!!";
				drawing = false;
			}

		}

		/**********************************************************************************************/
		//Comprobamos si se "pulsa un botón" comprobando que la mano esté sobre la imagen
		private void PushButton(HoverButton hoverButton, Image image)
		{
			if (IsItemMidPointInContainer(hoverButton, image))
			{
				hoverButton.Hovering();
			}
			else
			{
				hoverButton.Release();
			}
		}

		/**********************************************************************************************/
		//Comprobamos que el punto medio de la imagen que movemos (en este caso, la mano) esté dentro de la región que ocupa el botón (en este caso, otra imagen)
		public static bool IsItemMidPointInContainer(FrameworkElement container, FrameworkElement target)
		{
			FindValues(container, target);

			if (_itemTop < _topBoundary || _bottomBoundary < _itemTop)
			{
				//El centro está fuera los límites de arriba y abajo
				return false;
			}

			if (_itemLeft < _leftBoundary || _rightBoundary < _itemLeft)
			{
				//El centro está fuera los límites de izquierda y derecha
				return false;
			}

			return true;
		}

		/**********************************************************************************************/
		private static void FindValues(FrameworkElement container, FrameworkElement target)
		{
			var containerTopLeft = container.PointToScreen(new Point());
			var itemTopLeft = target.PointToScreen(new Point());

			_topBoundary = containerTopLeft.Y;
			_bottomBoundary = _topBoundary + container.ActualHeight;
			_leftBoundary = containerTopLeft.X;
			_rightBoundary = _leftBoundary + container.ActualWidth;

			//Usamos el centro del elemento
			_itemLeft = itemTopLeft.X + (target.ActualWidth / 2);
			_itemTop = itemTopLeft.Y + (target.ActualHeight / 2);
		}

		/**********************************************************************************************/
		//Escalamos la posición de la articulación para que podamos alcanzar toda la pantalla
		private void ScalePosition(FrameworkElement element, Joint joint)
		{
			Joint scaled = joint.ScaleTo((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, .3f, .3f);

			Canvas.SetLeft(element, scaled.Position.X);
			Canvas.SetTop(element, scaled.Position.Y);
		}

		/**********************************************************************************************/
		//Apagado de sensor
		private void sensorOff()
		{
			if (null != this.sensor)
			{
				this.sensor.AudioSource.Stop();

				this.sensor.Stop();
			}
		}

		/**********************************************************************************************/
        //Paramos el reconocedor de habla
        private void speechOff()
        {
            if (null != this.speechEngine)
            {
                this.speechEngine.SpeechRecognized -= speechEngine_SpeechRecognized;
                this.speechEngine.RecognizeAsyncStop();
            }
        }

        /**********************************************************************************************/
		//Tareas para apagar el dispositivo
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			sensorOff();
            speechOff();
		}

		/**********************************************************************************************/
		//Añadimos un punto a nuestro vector de puntos a dibujar
		void addPoint(System.Drawing.Point p)
		{
			System.Drawing.Point[] aux = new System.Drawing.Point[points.Length + 1];
			points.CopyTo(aux, 0);
			points = aux;
			points[points.Length - 1] = p;
		}

		/**********************************************************************************************/
		//Realizamos el dibujo en dos graphicos, uno para el pictureBox y otro en la imagen
		void draw()
		{
			if (points.Length > 1)
			{
				System.Drawing.Graphics g1 = paintPanel.CreateGraphics();
				System.Drawing.Graphics g2 = System.Drawing.Graphics.FromImage(bitmap);

				//Dibujamos
				g1.DrawLines(pen, points);
				g2.DrawLines(pen, points);
				
				//Liberamos recursos
				g1.Dispose();
				g2.Dispose();
			}
		}

		
		/**********************************************************************************************/
		//Evento de ratón cuando se reconoce la mano izquierda por debajo de la cintura
		void paintPanel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			//No se pinta
			drawing = false;

			//Reinicializamos puntos para que no se unan las lineas
			points = new System.Drawing.Point[0];

			//Marcamos como transparente en la imagen donde estamos dibujando todo aquello que sea del color de fondo, esto es necesario para que al cambiar de fondo no se vea lo que borramos
			bitmap.MakeTransparent(paintPanel.BackColor);
		}

		/**********************************************************************************************/
		//Evento de ratón cuando se reconoce el movimiento de la mano derecha
		void paintPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			//Si la mano izquierda está por encima de la cintura
			if (drawing)
			{
				//Añadimos el nuevo punto reconocido y pintamos
				addPoint(new System.Drawing.Point(e.X, e.Y));
				if (action == "dibujar")
				{
					draw();
				}			
			}
		}

		/**********************************************************************************************/
		//Evento de ratón cuando se reconoce la mano izquierda por encima de la cintura
		void paintPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			drawing = true;
		}
		
		/**********************************************************************************************/
		//Método para usar el ratón con el movimiento de la mano derecha
		private void GetCameraPoint(Skeleton first, AllFramesReadyEventArgs e)
		{
			//Usamos el sensor de profundidad
			using (DepthImageFrame depth = e.OpenDepthImageFrame())
			{
				if (depth == null || this.sensor == null)
				{
					return;
				}

				//Mano derecha
				DepthImagePoint rightHand = depth.MapFromSkeletonPoint(first.Joints[JointType.HandRight].Position);

				ColorImagePoint rightColorPoint = depth.MapToColorImagePoint(rightHand.X, rightHand.Y, ColorImageFormat.RgbResolution640x480Fps30);

				//Hacemos que la imagen siga la mano
				CameraPosition(imageHand, rightColorPoint);

				//Método de la clase NativeMethods a la que enviamos las posiciones y tamaños de pantalla para controlar el ratón
				NativeMethods.SendMouseInput(rightColorPoint.X + 50, rightColorPoint.Y + 50, (int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight, drawing);
			
			}
		}

		/**********************************************************************************************/
		//Método para centrar la imagen a la mano (joint)
		private void CameraPosition(FrameworkElement element, ColorImagePoint rightColorPoint)
		{
			Canvas.SetLeft(element, rightColorPoint.X - element.Width / 2);
			Canvas.SetTop(element, rightColorPoint.Y - element.Height / 2);
		}
		
		/**********************************************************************************************/
		//Método para reconocer el primer skeleton válido
		private Skeleton GetFirstSkeleton(AllFramesReadyEventArgs e)
		{
			using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
			{
				if (skeletonFrameData == null)
				{
					return null;
				}

				skeletonFrameData.CopySkeletonDataTo(allSkeleton);

				Skeleton first = (from s in allSkeleton where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();

				return first;
			}
		}

		/**********************************************************************************************/
		//Método para guardar lo que pintamos como fichero PNG
		private void saveImage()
		{
			//Nombre del fichero compuesto de IMG y la fecha del sistema
			String fileName = ".\\IMG_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

			//Informamos al usuario de lo que sucede
			try
			{
				bitmap.Save(fileName);
				textBlock2.Text = "Imagen guardada con éxito";
			}
			catch (Exception)
			{
				textBlock2.Text = "Hubo un problema al guardar la imagen";
			}


		}
		
		/**********************************************************************************************/
        //Obtiene los metadatos para el reconocedor de voz (modelo acústico) más adecuado para procesar el audio desde el dispositivo Kinect.
        private RecognizerInfo GetKinectRecognizer()
        {
            //Para todos los reconocedores de habla instalados busca reconocedor en espanol
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);

                //Busca el que coincida con lo siguiente
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "es-ES".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    recognizerES = true;
                    return recognizer;
                }
            }

            //Para todos los reconocedores de habla instalados busca reconocedor en ingles
            foreach (RecognizerInfo recognizer in SpeechRecognitionEngine.InstalledRecognizers())
            {
                string value;
                recognizer.AdditionalInfo.TryGetValue("Kinect", out value);

                //Busca el que coincida con lo siguiente
                if ("True".Equals(value, StringComparison.OrdinalIgnoreCase) && "en-US".Equals(recognizer.Culture.Name, StringComparison.OrdinalIgnoreCase))
                {
                    recognizerES = false;
                    return recognizer;
                }
            }

            return null;
        }

        /**********************************************************************************************/
        //Cargamos la gramática desde el fichero que está en bin->debug
        private Grammar CreateGrammarFromStream()
        {
            string fileName;

			//Si el reconocedor es el español, cargaremos la gramática española, en caso contrario, cargaremos la inglesa
            if (recognizerES)
                fileName = @"SpeechGrammarES.xml";
            else
                fileName = @"SpeechGrammarEN.xml";

            Grammar myGrammar = new Grammar(new FileStream(fileName, FileMode.Open));

            return myGrammar;
        }

        /**********************************************************************************************/
        //Tareas a realizar cuando el dispositivo reconozca una palabra (cambios de color en el trazo del dibujo)
        void speechEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            //Umbral de confianza
            const double ConfidenceThreshold = 0.65;

			//Si la confianza del resultado obtenido es mayor o igual a nuestro umbral...
			if (e.Result.Confidence >= ConfidenceThreshold)
			{

				switch (e.Result.Semantics.Value.ToString())
				{
					case "NEGRO":
						//Posicionamos la flecha y cambiamos el color del pincel a negro
						Canvas.SetTop(imageArrow, posYArrow);
						pen = new System.Drawing.Pen(System.Drawing.Color.Black, 5);
						break;

					case "GRIS":
						//Posicionamos la flecha y cambiamos el color del pincel a Gris
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 1));
						pen = new System.Drawing.Pen(System.Drawing.Color.DarkGray, 5);
						break;

					case "MORADO":
						//Posicionamos la flecha y cambiamos el color del pincel a Morado
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 2));
						pen = new System.Drawing.Pen(System.Drawing.Color.Purple, 5);
						break;

					case "VERDE":
						//Posicionamos la flecha y cambiamos el color del pincel a Verde
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 3));
						pen = new System.Drawing.Pen(System.Drawing.Color.Green, 5);
						break;

					case "ROJO":
						//Posicionamos la flecha y cambiamos el color del pincel a Rojo
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 4));
						pen = new System.Drawing.Pen(System.Drawing.Color.Red, 5);
						break;

					case "AZUL":
						//Posicionamos la flecha y cambiamos el color del pincel a azul
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 5));
						pen = new System.Drawing.Pen(System.Drawing.Color.Blue, 5);
						break;

					case "NARANJA":
						//Posicionamos la flecha y cambiamos el color del pincel a naranja
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 6));
						pen = new System.Drawing.Pen(System.Drawing.Color.OrangeRed, 5);
						break;

					case "AMARILLO":
						//Posicionamos la flecha y cambiamos el color del pincel a amarillo
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 7));
						pen = new System.Drawing.Pen(System.Drawing.Color.Yellow, 5);
						break;

					case "BLANCO":
						//Posicionamos la flecha y cambiamos el color del pincel a blanco
						Canvas.SetTop(imageArrow, posYArrow + (labelBlack.Height * 8));
						pen = new System.Drawing.Pen(System.Drawing.Color.White, 5);
						break;

					case "BORRAR":
						//Borramos la pizarra
						paintPanel.ForeColor = System.Drawing.Color.White;
						bitmap = new System.Drawing.Bitmap(paintPanel.Width, paintPanel.Height);
						break;

				}
			}
        }
	}
}
