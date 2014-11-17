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
using System.IO;


namespace GesturesDetector
{
	/// <summary> Actualización
	/// Lógica de interacción para MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		/**********************************************************************************************/
		//Ancho de imagen
		private const float RenderWidth = 640.0f;

		//Alto de imagen
		private const float RenderHeight = 480.0f;

		//Grosor de lineas de articulacion
		private const double JointThickness = 3;

		//Grosor de elipses
		private const double BodyCenterThickness = 10;

		//Grosor de lineas
		private const double ClipBoundsThickness = 10;

		//Brocha para el punto central del skeleton
		private readonly Brush centerPointBrush = Brushes.Blue;

		//Brocha para las articulaciones "seguidas correctamente" (tracked)
		private readonly Brush trackedJointBrush = Brushes.White;

		//Brocha para las articulaciones "no seguidas correctamente" (inferred)
		private readonly Brush inferredJointBrush = Brushes.Yellow;

		//Lápiz para dibujar huesos que son seguidos
		private Pen trackedBonePen = new Pen(Brushes.Purple, 6);

		//Lápiz azul, verde, amarillo, rojo
		private readonly Pen bluePen = new Pen(Brushes.Blue, 6);
		private readonly Pen greenPen = new Pen(Brushes.Green, 6);
		private readonly Pen yellowPen = new Pen(Brushes.Yellow, 6);
		private readonly Pen redPen = new Pen(Brushes.Red, 6);
		private readonly Pen whitePen = new Pen(Brushes.White, 6);

		//Lápiz para dibujar brazos que son seguidos
		private Pen trackedArmRightPen = new Pen(Brushes.Blue, 6);

		//Lápiz para dibujar brazos que son seguidos
		private Pen trackedArmLeftPen = new Pen(Brushes.Blue, 6);

		//Pen para pintar la pierna izquierda
		private Pen trackedLegLPen = new Pen(Brushes.Purple, 6);

		//Pen para pintar la pierna derecha
		private Pen trackedLegRPen = new Pen(Brushes.Purple, 6);

		//Variable tiempo para almacenar en que momento se salto
		private DateTime dateJump;

		//Variable para controlar si esta sentado o no
		private bool sitting = false;

		//Variables para almacenar la posicion de los pies cuando se sienta
		private float footR_inicial;
		private float footL_inicial;

		//Lápiz para dibujar huesos que no son seguidos
		private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

		//Instancia del sensor
		private KinectSensor sensor;

		//Grupo de dibujo para el renderizado del skeleton
		private DrawingGroup drawingGroup;

		//Imagen que se mostrará
		private DrawingImage imageSource;

		//Estructura para almacenar la posición de miembros
		public struct Position3
		{
			public float x;
			public float y;
			public float z;
			public DateTime date;
		};

		//Constantes para controlar movimiento y salto
		const float movMinimalLength = 0.2f;
		const float jumpMinimalLength = 0.2f;
		const int jumpMininalDuration = 100;
		const int jumpMaximalDuration = 500;
		const int movMininalDuration = 500;
		const int movMaximalDuration = 1500;
		const float threshold = 0.05f;
		const int MinimalPeriodBetweenGestures = 1500;

		public enum Gesture
		{
			None,
			SwipeToR,
			SwipeToL,
			SwipeToU,
			SwipeToD,
		}

		//Listas para llevar las posiciones de las manos y tronco (para el salto) obtenidas.
		List<Position3> positionHandRightList = new List<Position3>();
		List<Position3> positionHandLeftList = new List<Position3>();
		List<Position3> positionList = new List<Position3>();

		//Almacenamos gesto y tiempo en el que ha ocurrido.
		DateTime lastGestureRightDate = DateTime.Now;
		DateTime lastGestureLeftDate = DateTime.Now;
		DateTime compGestDate;
		Gesture gestureRight = new Gesture();
		Gesture gestureLeft = new Gesture();
		/**********************************************************************************************/

		public MainWindow()
		{
			InitializeComponent();
		}

		/**********************************************************************************************/
		//Ejecución de tareas de inicialización
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{

			// Creamos el drawingGroup que usaremos
			this.drawingGroup = new DrawingGroup();

			// Creamos la fuente de la imagen que usaremos en nuestro control de la imagen
			this.imageSource = new DrawingImage(this.drawingGroup);

			//Muestra el drawingGroup
			image1.Source = this.imageSource;

			//Inicializamos el sensor
			foreach (var potentialSensor in KinectSensor.KinectSensors)
			{
				if (potentialSensor.Status == KinectStatus.Connected)
				{
					this.sensor = potentialSensor;
					break;
				}
			}

			if (null != this.sensor)
			{
				//Activamos skeletonStream
				this.sensor.SkeletonStream.Enable();

				gestureRight = Gesture.None;
				gestureLeft = Gesture.None;

				//Añadimos un manejador de eventos que se llamará cada vez que se actualicen los frames
				this.sensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(sensor_SkeletonFrameReady);

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
		//Tareas para apagar el dispositivo
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			sensorOff();
		}

		/**********************************************************************************************/
		//Cálculo de la distancia según sean puntos positivos o negativos
		float Distance(float a, float b)
		{
			float sol = 0.0f;

			//Si ambos son positivos o negativos, la distancia es la diferencia en valor absoluto
			if (a * b > 0.0) sol = Math.Abs(a - b);
			//Si tienen distinto signo, la distancia es la suma de ambos (el negativo en valor absoluto)
			else sol = Math.Abs(a) + Math.Abs(b);

			return (sol);
		}

		/**********************************************************************************************/
		//Cálculo de dirección de movimiento
		int MovDir(float a, float b)
		{
			//Si la segunda posición es mayor que la primera, el movimiento es hacia derecha o hacia arriba
			if (b > a) return (1);
			//Si la primera posición es mayor, el movimiento es hacia izquierda o hacia abajo
			else if (b < a) return (-1);
			else return (0);
		}

		/**********************************************************************************************/
		//Las dos piernas abajo (por defecto)
		bool legDefault(Position3 footRight, Position3 footLeft)
		{
			float dist = Math.Abs(footRight.y - footLeft.y);

			if (dist < 0.15)
				return (true);
			else
				return (false);
		}

		/**********************************************************************************************/
		//La pierna derecha arriba
		bool legRightUp(Position3 footRight, Position3 footLeft)
		{
			float dist = footRight.y - footLeft.y;

			if ((dist > 0.15) && !sitting)
				return (true);
			else
				return (false);
		}

		/**********************************************************************************************/
		//La pierna izquierda arriba
		bool legLeftUp(Position3 footRight, Position3 footLeft)
		{
			float dist = footLeft.y - footRight.y;

			if ((dist > 0.15) && !sitting)
				return (true);
			else
				return (false);
		}

		/**********************************************************************************************/
		//Las dos piernas arriba (en silla)
		bool twoLegsUp(Position3 kneeRight, Position3 kneeLeft, Position3 hipRight, Position3 hipLeft, Position3 footRight, Position3 footLeft)
		{
			float distR = hipRight.y - kneeRight.y;
			float distL = hipLeft.y - kneeLeft.y;

			if (distR < 0.35 && distL < 0.35)
			{
				if (!sitting)
				{
					footR_inicial = footRight.y;
					footL_inicial = footLeft.y;
					sitting = true;

				}

				if ((footRight.y - footR_inicial) > 0.10 && (footLeft.y - footL_inicial) > 0.10)
					return true;
				else
					return false;
			}

			sitting = false;
			return false;

		}

		/**********************************************************************************************/
		//Las dos piernas arriba (saltando)
		bool jump()
		{
			int start = 0;
			for (int i = 1; i < positionList.Count - 1; i++)
			{
				if (MovDir(positionList[i - 1].y, positionList[i].y) < 0)
					start = i;

				if ((Distance(positionList[start].y, positionList[i].y) > movMinimalLength))
				{
					double totalMilliseconds = (positionList[i].date - positionList[start].date).TotalMilliseconds;
					if (totalMilliseconds >= jumpMininalDuration && totalMilliseconds <= jumpMaximalDuration)
					{
						positionList.Clear();
						dateJump = DateTime.Now;
						return true;
					}
				}
			}
			return false;
		}

		/**********************************************************************************************/
		//Ocultamos todos los ticks mostrados
		void HiddenAllTick()
		{
			imRD.Visibility = System.Windows.Visibility.Hidden;
			imLD.Visibility = System.Windows.Visibility.Hidden;
			imRU.Visibility = System.Windows.Visibility.Hidden;
			imLU.Visibility = System.Windows.Visibility.Hidden;
			imDR.Visibility = System.Windows.Visibility.Hidden;
			imDL.Visibility = System.Windows.Visibility.Hidden;
			imUR.Visibility = System.Windows.Visibility.Hidden;
			imUL.Visibility = System.Windows.Visibility.Hidden;

		}

		/**********************************************************************************************/
		//Gesto a detectar. Movimiento hacia abajo
		void SwipeToDown(List<Position3> posList, ref Gesture gest, ref DateTime lastGest)
		{
			//Posición inicial
			int start = 0;

			//Para todas las posiciones de la lista...
			for (int i = 1; i < posList.Count - 1; i++)
			{
				//Si se supera el umbral de desviación del movimiento o se detecta que la dirección del movimiento es errónea, se toma como
				//primera posición esta última.
				if ((Distance(posList[0].x, posList[i].x) > threshold) || (MovDir(posList[i - 1].y, posList[i].y) > 0))
				{
					start = i;
				}

				//Si se supera la distancia mínima entre las posiciones
				if ((Distance(posList[start].y, posList[i].y) > movMinimalLength))
				{
					double totalMilliseconds = (posList[i].date - posList[start].date).TotalMilliseconds;

					//Si el gesto se hace dentro de los límites de tiempo preestablecidos, reconocemos el gesto
					if (totalMilliseconds >= movMininalDuration && totalMilliseconds <= movMaximalDuration)
					{
						//Si dos gestos consecutivos se hacen dentro de un tiempo preestablecido y el gesto que precede es algún Swipe
						if (DateTime.Now.Subtract(lastGest).TotalMilliseconds < MinimalPeriodBetweenGestures && gest != Gesture.None)
						{
							//Ocultamos ticks
							HiddenAllTick();

							//Dependiendo del gesto predecesor será un movimiento complejo u otro
							switch (gest)
							{
								case Gesture.SwipeToR: imRD.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
								case Gesture.SwipeToL: imLD.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
							}
						}

						//Almacenamos el gesto para las siguientes comprobaciones
						gest = Gesture.SwipeToD;
						lastGest = DateTime.Now;
						posList.Clear();
					}
				}

			}
		}

		/**********************************************************************************************/
		//Gesto a detectar. Movimiento hacia arriba
		void SwipeToUp(List<Position3> posList, ref Gesture gest, ref DateTime lastGest)
		{
			int start = 0;
			for (int i = 1; i < posList.Count - 1; i++)
			{
				if ((Distance(posList[0].x, posList[i].x) > threshold) || (MovDir(posList[i - 1].y, posList[i].y) < 0))
				{
					start = i;
				}

				if (Distance(posList[start].y, posList[i].y) > movMinimalLength)
				{
					double totalMilliseconds = (posList[i].date - posList[start].date).TotalMilliseconds;

					if (totalMilliseconds >= movMininalDuration && totalMilliseconds <= movMaximalDuration)
					{
						if (DateTime.Now.Subtract(lastGest).TotalMilliseconds < MinimalPeriodBetweenGestures && gest != Gesture.None)
						{
							HiddenAllTick();

							switch (gest)
							{
								case Gesture.SwipeToR: imRU.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
								case Gesture.SwipeToL: imLU.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
							}
						}

						gest = (Gesture.SwipeToU);
						lastGest = DateTime.Now;
						posList.Clear();

					}
				}
			}
		}

		/**********************************************************************************************/
		//Gesto a detectar. Movimiento hacia la derecha
		void SwipeToRight(List<Position3> posList, ref Gesture gest, ref DateTime lastGest)
		{
			int start = 0;
			for (int i = 1; i < posList.Count - 1; i++)
			{
				if ((Distance(posList[0].y, posList[i].y) > threshold) || (MovDir(posList[i - 1].x, posList[i].x) < 0))
				{
					start = i;
				}

				if ((Distance(posList[start].x, posList[i].x) > movMinimalLength))
				{
					double totalMilliseconds = (posList[i].date - posList[start].date).TotalMilliseconds;
					if (totalMilliseconds >= movMininalDuration && totalMilliseconds <= movMaximalDuration)
					{
						if (DateTime.Now.Subtract(lastGest).TotalMilliseconds < MinimalPeriodBetweenGestures && gest != Gesture.None)
						{
							HiddenAllTick();

							switch (gest)
							{
								case Gesture.SwipeToU: imUR.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
								case Gesture.SwipeToD: imDR.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
							}
						}

						gest = (Gesture.SwipeToR);
						lastGest = DateTime.Now;
						posList.Clear();
					}
				}
			}
		}

		/**********************************************************************************************/
		//Gesto a detectar. Movimiento hacia la izquierda
		void SwipeToLeft(List<Position3> posList, ref Gesture gest, ref DateTime lastGest)
		{
			int start = 0;
			for (int i = 1; i < posList.Count - 1; i++)
			{
				if ((Distance(posList[0].y, posList[i].y) > threshold) || (MovDir(posList[i - 1].x, posList[i].x) > 0))
				{
					start = i;
				}

				if ((Distance(posList[start].x, posList[i].x) > movMinimalLength))
				{
					double totalMilliseconds = (posList[i].date - posList[start].date).TotalMilliseconds;
					if (totalMilliseconds >= movMininalDuration && totalMilliseconds <= movMaximalDuration)
					{
						if (DateTime.Now.Subtract(lastGest).TotalMilliseconds < MinimalPeriodBetweenGestures && gest != Gesture.None)
						{
							HiddenAllTick();

							switch (gest)
							{
								case Gesture.SwipeToU: imUL.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
								case Gesture.SwipeToD: imDL.Visibility = System.Windows.Visibility.Visible; compGestDate = DateTime.Now;
									break;
							}
						}

						gest = (Gesture.SwipeToL);
						lastGest = DateTime.Now;
						posList.Clear();
					}
				}
			}
		}

		/**********************************************************************************************/
		//Mano por debajo de la cintura
		void handDefault(Position3 hRight, Position3 hLeft, Position3 hCenter)
		{
			//si alguna de las manos está por debajo de la cintura
			if (hRight.y < hCenter.y)
			{
				trackedArmRightPen = bluePen;
			}

			if (hLeft.y < hCenter.y)
			{
				trackedArmLeftPen = bluePen;
			}
		}

		/**********************************************************************************************/
		//Mano por encima de la cintura
		void handOverHip(Position3 hRight, Position3 hLeft, Position3 hCenter)
		{
			//si alguna de las manos está por encima de la cintura
			if (hRight.y > hCenter.y)
			{
				trackedArmRightPen = greenPen;
			}

			if (hLeft.y > hCenter.y)
			{
				trackedArmLeftPen = greenPen;
			}
		}

		/**********************************************************************************************/
		//Mano por encima del hombro
		void handOverShoulder(Position3 hRight, Position3 hLeft, Position3 shoulRight, Position3 shoulLeft)
		{
			//si alguna de las manos está por encima del hombro
			if (hRight.y > shoulRight.y)
			{
				trackedArmRightPen = yellowPen;
			}

			if (hLeft.y > shoulLeft.y)
			{
				trackedArmLeftPen = yellowPen;
			}
		}

		/**********************************************************************************************/
		//Mano por encima de la cabeza
		void handOverHead(Position3 hRight, Position3 hLeft, Position3 headCenter)
		{
			//si alguna de las manos está por encima de la cabeza
			if (hRight.y > headCenter.y)
			{
				trackedArmRightPen = redPen;
			}

			if (hLeft.y > headCenter.y)
			{
				trackedArmLeftPen = redPen;
			}
		}

		/**********************************************************************************************/
		//Almacenamos la posición de la articulación que le pasemos
		void PosDetecter(ref Position3 pos, Joint joint)
		{
			pos.x = joint.Position.X;
			pos.y = joint.Position.Y;
			pos.z = joint.Position.Z;
			pos.date = DateTime.Now;
		}

		/**********************************************************************************************/
		//Obtener posiciones de las joints deseadas
		void JointsTracked(Skeleton skel)
		{
			//Obtenemos posicion de mano derecha
			Position3 handRight = new Position3();
			PosDetecter(ref handRight, skel.Joints[JointType.HandRight]);

			//Obtenemos posicion de mano izquierda
			Position3 handLeft = new Position3();
			PosDetecter(ref handLeft, skel.Joints[JointType.HandLeft]);

			//Obtenemos posicion de cadera
			Position3 hipCenter = new Position3();
			PosDetecter(ref hipCenter, skel.Joints[JointType.HipCenter]);

			//Obtenemos posicion de hombro derecho
			Position3 shoulderRight = new Position3();
			PosDetecter(ref shoulderRight, skel.Joints[JointType.ShoulderRight]);

			//Obtenemos posicion de hombro izquierdo
			Position3 shoulderLeft = new Position3();
			PosDetecter(ref shoulderLeft, skel.Joints[JointType.ShoulderLeft]);

			//Obtenemos posicion de cabeza
			Position3 head = new Position3();
			PosDetecter(ref head, skel.Joints[JointType.Head]);

			//Obtenemos posicion del pie derecho
			Position3 footRight = new Position3();
			PosDetecter(ref footRight, skel.Joints[JointType.FootRight]);

			//Obtenemos posicion del pie izquierda
			Position3 footLeft = new Position3();
			PosDetecter(ref footLeft, skel.Joints[JointType.FootLeft]);

			//Obtenemos posicion de cadera derecha
			Position3 hipRight = new Position3();
			PosDetecter(ref hipRight, skel.Joints[JointType.HipRight]);

			//Obtenemos posicion de cadera izquierda
			Position3 hipLeft = new Position3();
			PosDetecter(ref hipLeft, skel.Joints[JointType.HipLeft]);

			//Obtenemos posicion de la rodilla derecha
			Position3 kneeRight = new Position3();
			PosDetecter(ref kneeRight, skel.Joints[JointType.KneeRight]);

			//Obtenemos posicion de la rodilla izquierda
			Position3 kneeLeft = new Position3();
			PosDetecter(ref kneeLeft, skel.Joints[JointType.KneeLeft]);

			//Obtenemos posicion de la columna para controlar el salto.
			Position3 spine = new Position3();
			PosDetecter(ref spine, skel.Joints[JointType.Spine]);

			//Guardamos en listas espeíficas las posiciones que trataremos en el control de gestos
			positionHandRightList.Add(handRight);
			positionHandLeftList.Add(handLeft);
			positionList.Add(spine);

			//Condición para que cuando saltamos, se mantenga por un momento el color de ambas piernas en blanco
			if ((DateTime.Now - dateJump).TotalMilliseconds > 1500)
				if (legDefault(footRight, footLeft))
					trackedLegRPen = trackedLegLPen = bluePen;

			//Si levantamos la pierna derecha, la pintamos de verde
			if (legRightUp(footRight, footLeft))
				trackedLegRPen = greenPen;

			//Si levantamos la pierna izquierda, la pintamos de amarillo
			if (legLeftUp(footRight, footLeft))
				trackedLegLPen = yellowPen;

			//Si sentados, levantamos ambas piernas, las pintamos de rojo
			if (twoLegsUp(kneeRight, kneeLeft, hipRight, hipLeft, footRight, footLeft))
				trackedLegRPen = trackedLegLPen = redPen;

			//Si saltamos, pintamos las piernas de blanco
			if (jump())
				trackedLegLPen = trackedLegRPen = whitePen;

			//Pintamos el skeleton de azul si corresponde
			handDefault(handRight, handLeft, hipCenter);

			//Pintamos el skeleton de verde si corresponde
			handOverHip(handRight, handLeft, hipCenter);

			//Pintamos el skeleton de amarillo si corresponde
			handOverShoulder(handRight, handLeft, shoulderRight, shoulderLeft);

			//Pintamos el skeleton de rojo si corresponde
			handOverHead(handRight, handLeft, head);

			//Condición para que espere un período de tiempo después de captar un gesto compuesto
			if ((DateTime.Now - compGestDate).TotalMilliseconds > MinimalPeriodBetweenGestures)
			{
				SwipeToRight(positionHandLeftList, ref gestureLeft, ref lastGestureLeftDate);
				SwipeToLeft(positionHandLeftList, ref gestureLeft, ref lastGestureLeftDate);
				SwipeToUp(positionHandLeftList, ref gestureLeft, ref lastGestureLeftDate);
				SwipeToDown(positionHandLeftList, ref gestureLeft, ref lastGestureLeftDate);
				SwipeToRight(positionHandRightList, ref gestureRight, ref lastGestureRightDate);
				SwipeToLeft(positionHandRightList, ref gestureRight, ref lastGestureRightDate);
				SwipeToUp(positionHandRightList, ref gestureRight, ref lastGestureRightDate);
				SwipeToDown(positionHandRightList, ref gestureRight, ref lastGestureRightDate);
			}

			//Controlamos que las listas no se hagan demasiado grandes eliminando la primera posición
			if (positionHandRightList.Count() > 20)
				positionHandRightList.RemoveAt(0);

			if (positionHandLeftList.Count() > 20)
				positionHandLeftList.RemoveAt(0);

			if (positionList.Count() > 20)
				positionList.RemoveAt(0);
		}

		/**********************************************************************************************/
		//Tareas a realizar cuando el dispositivo reconozca un cambio en la imagen
		void sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
		{
			Skeleton[] skeletons = new Skeleton[0];

			using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
			{
				if (skeletonFrame != null)
				{
					skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
					skeletonFrame.CopySkeletonDataTo(skeletons);
				}
			}

			using (DrawingContext dc = this.drawingGroup.Open())
			{
				//Dibujamos todo el fondo de la imagen de color negro
				dc.DrawRectangle(Brushes.Black, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));

				if (skeletons.Length != 0)
				{
					//Para cada skeleton reconocido
					foreach (Skeleton skel in skeletons)
					{

						//Si el skeleton es captado en su mayor parte
						if (skel.TrackingState == SkeletonTrackingState.Tracked)
						{
							//Obtener posiciones de joints deseadas
							JointsTracked(skel);

							//Dibujamos el skeleton con sus huesos y articulaciones
							this.DrawBonesAndJoints(skel, dc);
						}

						//Si no, si solo captamos la posición
						else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
						{
							//Dibujamos un círculo en el centro del skeleton
							dc.DrawEllipse(
							this.centerPointBrush,
							null,
							this.SkeletonPointToScreen(skel.Position),
							BodyCenterThickness,
							BodyCenterThickness);
						}
					}
				}

				//Evitamos que se dibuje fuera de nuestra área de dibujo
				this.drawingGroup.ClipGeometry = new RectangleGeometry(new Rect(0.0, 0.0, RenderWidth, RenderHeight));
			}
		}

		/**********************************************************************************************/
		//Dibuja los huesos y articulaciones del esqueleto
		private void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
		{
			//Torso
			this.DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter, trackedBonePen);//Cabeza-Cuello
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft, trackedBonePen);//Cuello-HombroIzq
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight, trackedBonePen);//Cuello-HombroDer
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine, trackedBonePen);//Cuello-Espinazo
			this.DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter, trackedBonePen);//Espinazo-CentroCadera
			this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft, trackedBonePen);//CentroCadera-CaderaIzq
			this.DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight, trackedBonePen);//CentroCadera-CaderaDer

			//Brazo izquierdo
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderLeft, JointType.ElbowLeft, trackedBonePen);//HombroIzq-Codo
			this.DrawBone(skeleton, drawingContext, JointType.ElbowLeft, JointType.WristLeft, trackedArmLeftPen);//Codo-Muñeca
			this.DrawBone(skeleton, drawingContext, JointType.WristLeft, JointType.HandLeft, trackedArmLeftPen);//Muñeca-Mano

			//Brazo derecho
			this.DrawBone(skeleton, drawingContext, JointType.ShoulderRight, JointType.ElbowRight, trackedBonePen);//HombroDer-Codo
			this.DrawBone(skeleton, drawingContext, JointType.ElbowRight, JointType.WristRight, trackedArmRightPen);//Codo-Muñeca
			this.DrawBone(skeleton, drawingContext, JointType.WristRight, JointType.HandRight, trackedArmRightPen);//Muñeca-Mano

			//Pierna izquierda
			this.DrawBone(skeleton, drawingContext, JointType.HipLeft, JointType.KneeLeft, trackedBonePen);//CaderaIzq-Rodilla
			this.DrawBone(skeleton, drawingContext, JointType.KneeLeft, JointType.AnkleLeft, trackedLegLPen);//Rodilla-Tobillo
			this.DrawBone(skeleton, drawingContext, JointType.AnkleLeft, JointType.FootLeft, trackedLegLPen);//Tobillo-Pie

			//Pierna derecha
			this.DrawBone(skeleton, drawingContext, JointType.HipRight, JointType.KneeRight, trackedBonePen);//CaderaDer-Rodilla
			this.DrawBone(skeleton, drawingContext, JointType.KneeRight, JointType.AnkleRight, trackedLegRPen);//Rodilla-Tobillo
			this.DrawBone(skeleton, drawingContext, JointType.AnkleRight, JointType.FootRight, trackedLegRPen);//Tobillo-Pie

			//Articulaciones
			foreach (Joint joint in skeleton.Joints)
			{
				Brush drawBrush = null;

				if (joint.TrackingState == JointTrackingState.Tracked)
				{
					drawBrush = this.trackedJointBrush;
				}
				else if (joint.TrackingState == JointTrackingState.Inferred)
				{
					drawBrush = this.inferredJointBrush;
				}

				if (drawBrush != null)
				{
					drawingContext.DrawEllipse(drawBrush, null, this.SkeletonPointToScreen(joint.Position), JointThickness, JointThickness);
				}
			}
		}

		/**********************************************************************************************/
		//Escalamos puntos a pantalla
		private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
		{
			DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
			return new Point(depthPoint.X, depthPoint.Y);
		}

		/**********************************************************************************************/
		//Dibuja lineas entre articulaciones
		private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1, Pen colour)
		{
			Joint joint0 = skeleton.Joints[jointType0];
			Joint joint1 = skeleton.Joints[jointType1];

			//Si no encontramos ninguna de estas articulaciones, salimos
			if (joint0.TrackingState == JointTrackingState.NotTracked ||
				joint1.TrackingState == JointTrackingState.NotTracked)
			{
				return;
			}

			//No dibujamos si ambos puntos no son seguidos
			if (joint0.TrackingState == JointTrackingState.Inferred &&
				joint1.TrackingState == JointTrackingState.Inferred)
			{
				return;
			}

			//Dependiendo de si los huesos son captados o no, se dibujará en un color u otro
			Pen drawPen = this.inferredBonePen;
			if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
			{
				drawPen = colour;
			}

			drawingContext.DrawLine(drawPen, this.SkeletonPointToScreen(joint0.Position), this.SkeletonPointToScreen(joint1.Position));
		}
	}
}
