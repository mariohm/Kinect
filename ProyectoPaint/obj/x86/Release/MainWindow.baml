   M S B A M L   `   `   `  ���� P  LCoding4Fun.Kinect.Wpf, Version=1.7.0.0, Culture=neutral, PublicKeyToken=nullnKclr-namespace:Coding4Fun.Kinect.Wpf.Controls;assembly=Coding4Fun.Kinect.WpfCoding4Fun.Kinect.Wpf.Controls  ^ ZMicrosoft.Samples.Kinect.WpfViewers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null�^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers#Microsoft.Samples.Kinect.WpfViewers [ WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms  ProyectoPaint   PaintProyect.MainWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     `ControlsKclr-namespace:Coding4Fun.Kinect.Wpf.Controls;assembly=Coding4Fun.Kinect.Wpf     u
WpfViewers^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers     Owf@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     -     � Title$  Kinect Paint��$	��710��$	��988�� � 
ResizeMode$ CanResizeWithGrip=�.���� -   $��
MainCanvas��$��0q�$
��Auto��.+��� -    ��Name$ image��$
��Auto��
 �� Top$ 33��$	��602�� �� Left$ 32��$��Images/imagePizarra.jpg���� -   $ 	imageSave��$��94��$��92��$	 764��$ 12��$��Images/save.png���� -   $ 	imageExit��$��94��$��92��$	 764��$	 132��$��Images/close_icon.png���� -   $ 
imageSpain��$��94��$��92��$	 663��$	 338��$��Images/spain.png��$y�Hidden=��� -   $ 	imageEEUU��$��94��$��92��$	 663��$	 338��$��Images/united_states.png��$y�Hidden=��� -   $ 	imageHand��$��70��$��70��$	 786��$	 232��$��Images/hand.png���� -	   $��
imageArrow��$	 786��$	 340��$��33��$
��Fill=�$��46��$��Images/flecha.jpg��0   *Coding4Fun.Kinect.Wpf.Controls.HoverButton  -
     Name$ 	hoverSave��   	ImageSize$	 100Q�   TimeInterval$
 1000��$��94��$��92��$	 764��$ 11��$��0,1,1,0q�  -   $ 	hoverExit��$	 100Q�$
 1000��$��94��$��92��$	 764��$	 132���� -   $��
textBlock1��$��20(�$	 552��$	 558��$��42��$�� ��$	��381��7  1System.Windows.Forms.Integration.WindowsFormsHost  -   $��windowsFormsHost1��$ 49��$ 64��$	��401��$	��571��   Child.    System.Windows.Forms.Panel  -   
paintPanel���� -   $��
textBlock2��$��22(�$ 32��$	 556��$��44��$�� ��$	��347���� -   $��
labelBlack�����	#FF0D0D0D$��2q�$	 850��$	 340��$�� ��$��20��$	��100��
���   ��� -   $��	labelGray�����	#FF0D0D0D$��2q�$	 850��$	 360��$�� ��$��20��$	��100��
��������� -   $��
labePurple�����	#FF0D0D0D$��2q�$	 850��$	 380��$�� ��$��20��$	��100��
���� ���� -   $��
labelGreen�����	#FF0D0D0D$��2q�$	 850��$	 400��$�� ��$��20��$	��100��
��� � ��� -   $��labelRed�����	#FF0D0D0D$��2q�$	 850��$	 420��$�� ��$��20��$	��100��
���  ���� -   $��	labelBlue�����	#FF0D0D0D$��2q�$	 850��$	 440��$�� ��$��20��$	��100��
����  ��� -   $��labelOrange�����	#FF0D0D0D$��2q�$	 850��$	 460��$�� ��$��20��$	��100��
��� E���� -   $��labelYellow�����	#FF0D0D0D$��2q�$	 850��$	 480��$�� ��$��20��$	��100��
��� ����� -   $��
labelWhite�����	#FF0D0D0D$��2q�$	 850��$	 500��$�� ��$��20��$	��100��
�������