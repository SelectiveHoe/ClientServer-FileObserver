   M S B A M L   `   `   `  ����K  GWpfConnectClient, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:WpfConnectClientWpfConnectClient  5   	   !    WpfConnectClient.MainWindow   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   H DMahApps.Metro, Version=1.6.5.1, Culture=neutral, PublicKeyToken=null;Controls,http://metro.mahapps.com/winfx/xaml/controls  5   	   J FWpfAnimatedGif, Version=1.4.18.0, Culture=neutral, PublicKeyToken=null,gif"http://wpfanimatedgif.codeplex.com  5   	   Dmc;http://schemas.openxmlformats.org/markup-compatibility/2006  5   	   *localclr-namespace:WpfConnectClient  5   	   -   5          � Title$  Client: Connect��5
   	   $	��500��6!   $
��1000��6.   .��5      � -   5         �Name$ MainGrid��5       � RowDefinitions 5   
   �� 5      .+�5   
   �� ��5      �� 5       �� EndPoint$ 0.5,1&�6&    �� 
StartPoint$ 0.5,0&�67   .��5      �  � Color$ White��6#    � Offset$ 0.067Q�61   � 5      $ 	#FFECE0FD��6#   5      5      �� 5   
   -   5         ��Name$ LoadingAnim��5      "  WpfAnimatedGif.ImageBehavior   AnimatedSource$ ezgif.com-resize.gif��6%   	 J� Opacity$	 1Q�6]   $��50��6i   $��50��6t   6�   �� 5   
   -   5        
 ��Name$

 Tabs��5      .@�5      �� $��Connection Info��6   
������ .��5      �� $��Center=�6   $��Center=�6:   $��0,0,0,50q�6U   .+�5      �� $
��Left=�6   .��67   Connection IP:6G   �� 5      -   5         ��Name$ ConnectionIP��5   9   $
��Left=�6   $	��200��6O   $��5,0,5,0q�6[   6n   �� 5      $
��Left=�6   .��67   Connection Port:6I   �� 5      -   5        $ ConnectionPort��5   9   $
��Left=�6   $	��200��6Q   $��5,0,5,0q�6]   6p   �� 5       -   5        $��5q�5       $��Center=�6=   .��6Z   	Connect6c   5!      5#      �� 5$      $��File Manager��6   
������ .��5%      � -   5        $ FTPGrid��5%       5&      �� 5'      $��35 �6(   �� 5(      $��30 �6(   �� 5)      $��5 �6(   �� 5*      .+�5,      ��  �� Orientation$ 
Horizontal=�6!   .+�5-      �� -	   5        $��0,5,5,5q�5-   !   .��6I   Refresh FTP6V   �� 5.      -
   5        $��0,0,5,5q�5.   !   $��25��6I   $��Bottom=�6U   .��6p   Back Folder6}   5/      �� 50      -   5         ��Name$ LabelPathFolder��50      $��1��65   6D   �� 51      $��2��6    6/   �� 52      -   5         ��Name$ FolderTreeView��52      $��3��66    J� 	AllowDrop . ��53      �� 54      .R�55   "   � .+�56   &   �  � Maximum$	 100Q�6Z    � Minimum$ 0Q�6h   
������ 
������  � Value 62   ��  �� Path$ DownloadProgress�6�   �� 57   &   $ 
Horizontal=�61   $��Center=�6J   .+�58   *   �� $��18��60   $��18��6;    �� BitmapScalingMode$ HighQuality=�6d   ��6G   �� $
 icon�6d   �� 59   *   $��5,0,0,0q�64   $��12(�6E   $��Center=�6n   ��6S   �� $
 path�6n   5:   '   5;   #   5<      5>      5?      5@      �� 5A      $��Download Manager��6   
������ .��5B      �  5C      �� 5D      �� 5E      $��30 �6(   .+�5G      �� -   5        $ DMList��5G      ��5H      �� 5I      .R�5J   "   � $��5q�6'   $��20��62    � ColumnDefinitions 5K   &   �� 5L   *   �� 5M   *   $��30 �6;   �� 5N   *   $��30 �6;   �� 5O   *   $��30 �6;    5Q   &   �� 5R   *   �� 5S   *   $��2 �68   .+�5U   &   � $��1��62   $	 100Q�6j   $ 0Q�6x   $��Stretch=�6�   $��Bottom=�6�    6?   �� $ ProgressBarProccess�6�   �� 5V   &   -   5        $��2��5V   ,   $��1��6=   $ HighQuality=�6�   ��6M   �� $ pauseStopIcon�6�   �� 5W   &   -   5        $��2��5W   ,   $��2��6=   $��
folder.png��6M   $ HighQuality=�6{   �� 5X   &   -   5        $��2��5X   ,   $��3��6=   $��TrashBucket.png��6M   $ HighQuality=�6~   �� 5Y   &   $��5,0,0,0q�60   $��12(�6A   ��6O   �� $ PathOnServer�5Z   #   5[      5]      �� 5^      $��1��6!   $ 
Horizontal=�6.   $��Center=�6G   .+�5_      �� $��0,0,5,0q�6$   Current save folder:6K   �� 5`      -   5         ��Name$ 
SaveFolder��5`   $   6:   5a      �� 5b      -   5         ��Name$ BtnChangeSaveFolder��5b      $��1��6:   $��Right=�6G   $��Center=�6c   .��6�   Change folder6�   5c      5d      5e      5f      5g      