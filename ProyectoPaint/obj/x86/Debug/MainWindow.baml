   M S B A M L   `   `   `  ����P  LCoding4Fun.Kinect.Wpf, Version=1.7.0.0, Culture=neutral, PublicKeyToken=nullnKclr-namespace:Coding4Fun.Kinect.Wpf.Controls;assembly=Coding4Fun.Kinect.WpfCoding4Fun.Kinect.Wpf.Controls  5   	   ^ ZMicrosoft.Samples.Kinect.WpfViewers, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null�^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers#Microsoft.Samples.Kinect.WpfViewers 5   	   [ WSystem.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089Y@clr-namespace:System.Windows.Forms;assembly=System.Windows.FormsSystem.Windows.Forms 5   	    ProyectoPaint   PaintProyect.MainWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   `ControlsKclr-namespace:Coding4Fun.Kinect.Wpf.Controls;assembly=Coding4Fun.Kinect.Wpf     5   	   u
WpfViewers^clr-namespace:Microsoft.Samples.Kinect.WpfViewers;assembly=Microsoft.Samples.Kinect.WpfViewers     5   	   Owf@clr-namespace:System.Windows.Forms;assembly=System.Windows.Forms     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   _   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     6�   -   5          � Title$  Kinect Paint��5   	   $	��755��6�   $	��988��6�    � 
ResizeMode$ CanResizeWithGrip=�6�   .��5      �� -   5        $��
MainCanvas��5      $��0q�6   $
��Auto��6*   $	��722��67   .+�5	   
   �� -   5         ��Name$ image��5	      $
��Auto��6   
 �� Top$	 150��6-   $	��602��6>    �� Left$ 55��6J   $��Images/imagePizarra.jpg��6[   �� 5
   
   -   5        $ 	imageSave��5
      $��94��6#   $��92��6/   $	 839��6:   $ 73��6L   $��Images/save.png��6\   �� 5   
   -   5        $ 	imageExit��5      $��94��6#   $��92��6/   $	 839��6:   $	 194��6L   $��Images/close_icon.png��6]   �� 5   
   -   5        $ 
imageSpain��5      $��94��6$   $��92��60   $	 663��6;   $	 338��6M   $��Images/spain.png��6^   $y�Hidden=�6x   �� 5   
   -   5        $ 	imageEEUU��5      $��94��6#   $��92��6/   $	 663��6:   $	 338��6L   $��Images/united_states.png��6]   $y�Hidden=�6   �� 5   
   -   5        $ 	imageHand��5      $��70��6#   $��70��6/   $	 725��6:   $	 132��6L   $��Images/hand.png��6]   �� 5   
   -	   5        $��
imageArrow��5   ?   $	 796��6   $	 340��6"   $��20��63   $
��Fill=�6Q   $��46��6`   $��Images/flecha.jpg��6k   0   *Coding4Fun.Kinect.Wpf.Controls.HoverButton  5   
   -
   5          Name$ 	hoverSave��5         	ImageSize$	 100Q�62      TimeInterval$
 1000��6B   $��94��6V   $��92��6b   $	 839��6m   $ 72��6   $��0,1,1,0q�6�     5   
   -   5        $ 	hoverExit��5      $	 100Q�62   $
 1000��6B   $��94��6V   $��92��6b   $	 839��6m   $	 194��6   �� 5   
   -   5        $��
textBlock1��5   Q   $��20(�6   $	 539��6"   $	 632��64   $��42��6E   $�� ��6c   $	��381��6k   7  1System.Windows.Forms.Integration.WindowsFormsHost  5   
   -   5        $��windowsFormsHost1��5   J   $ 75��6   $	 181��6,   $	��401��6=   $	��571��6c      Child. 5         System.Windows.Forms.Panel  -   5        
paintPanel��5      5      �� 5   
   -   5        $��
textBlock2��5   P   $��22(�6   $ 49��6"   $	 632��63   $��44��6D   $�� ��6b   $	��347��6j   �� 5   
   -   5        $��
labelBlack��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 340��6N   $�� ��6_   $��20��6j   $	��100��6�   
���   �6�   �� 5   
   -   5        $��	labelGray��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 360��6N   $�� ��6_   $��20��6j   $	��100��6�   
�������6�   �� 5   
   -   5        $��
labePurple��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 380��6N   $�� ��6_   $��20��6j   $	��100��6�   
���� ��6�   �� 5   
   -   5        $��
labelGreen��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 400��6N   $�� ��6_   $��20��6j   $	��100��6�   
��� � �6�   �� 5   
   -   5        $��labelRed��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 420��6N   $�� ��6_   $��20��6j   $	��100��6�   
���  ��6�   �� 5   
   -   5        $��	labelBlue��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 440��6N   $�� ��6_   $��20��6j   $	��100��6�   
����  �6�   �� 5   
   -   5        $��labelOrange��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 460��6N   $�� ��6_   $��20��6j   $	��100��6�   
��� E��6�   �� 5   
   -   5        $��labelYellow��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 480��6N   $�� ��6_   $��20��6j   $	��100��6�   
��� ���6�   �� 5   
   -   5        $��
labelWhite��5   v   ���	#FF0D0D0D$��2q�6(   $	 850��6<   $	 500��6N   $�� ��6_   $��20��6j   $	��100��6�   
�������6�   �� 5    
   -   5        $��label1��5    �   $ 70��6   $
 -107��6!   $E��?1.- Observe como detecta Kinect si levanta alguna mano o pierna��63   $��20(�6}   $��42��6�   $	��587��6�   �� 5!   
   -   5        $��label2��5!   �   $ 59��6   $ 10��6!   $q��k1.- Levante la mano izquierda por encima de la cintura para poder dibujar con la mano derecha en la pizarra��61   $��18(�6�   $��35��6�   $	��872��6�   �� 5"   
   -   5        $��label3��5"   �   $ 59��6   $ 40��6!   $M��G2.- Observe la bandera para determinar el idioma del reconocedor de voz��61   $��18(�6�   $��35��6�   $	��598��6�   �� 5#   
   -   5        $��label4��5#   �   $ 59��6   $ 70��6!   $B��<3.- Para cambiar el color del lápiz diga el color que desee��61   $��18(�6w   $��35��6�   $	��587��6�   �� 5$   
   -   5        $��label5��5$   �   $ 59��6   $	 100��6!   $7��14.- Diga borrar/delete si desea borrar la pizarra��62   $��18(�6o   $��35��6}   $	��587��6�   5%      5&      