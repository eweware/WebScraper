   M S B A M L   `   `   `  ����   
WebScraper    WebScraper.MainWindow   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  
MainWindow��$	��480��$	��800�� � 	Resources    ImportItemTemplate � ColumnDefinitions d� 	IsChecked �� Mode �� Orientation � RowDefinitions �� TextWrapping �� ElementName	 �� Path"
 �� VerticalScrollBarVisibility �� AcceptsReturn��%  &	        �� .R��� 
���   �$��0 0 0 1q�._�� $
��Left=� �� $��24 ��� $	��150 ��� $��* �.+��� $��Center=�$��0��$��4q� �� *Upload+$ TwoWay=��� $��1��.+��� $��8q�$	��128��$	��128������ *
ImageURL+�� $ 
Horizontal=�.+��� $��Center=�$��4q� �� *
UseImage+$ TwoWay=��� 	use image� $��2��$��Stretch=� �� $
��auto ��� $
��auto � �� $��* ��� $
��auto �.+��� $��0��$��0��$��16(�$
 Wrap=����� $ userctrl��$	 ActualWidth����� *Title+$ TwoWay=��� $��0��$��1������ *Title.Length+$ OneWay=��� $��1��$��0��$��2��$	��256��$

 Auto=� . $
 Wrap=����� $ userctrl��$	 ActualWidth����� *Body+$ TwoWay=�.���  �� $
��Auto ��� $��* ��� $
��Auto �.+��� -    ��Name$ 
ResultList��$��Stretch=�$��1��#��[  $��Stretch=�$��Disabled=�� $��0��$��0 4 0 8q� �� $��* ��� $
��Auto ��� $��* ��� $��* �.+��� -   $��
DeviantArt��$��0��$	��8 2q�$	��8 0q�$��Center=�$	��Top=��� $ 
Horizontal=�$��1��.+��� -    ��Name$ 	RedditBox�� �� Text$ Reddit Feed��$	��100��.@��� -   $��
RedditTech��.��
Technology�� -   $��	RedditEnt��.��Entertainment�� -   $��	RedditInt��.��Interesting�� -    ��Name$ RedditString��$	��250���� -   $��Reddit Import��$��3��$	��8 2q�$��Center=�$	��Top=��� $ 
Horizontal=�$��2��$��0 8 0 4q�.+��� -	   $��Import to QA��$��0��$	��8 0q�$	��8 2q�$��Center=�$	��Top=��� -
   $��Import to PROD��$��0��$	��8 0q�$	��8 2q�$��Center=�$	��Top=��� $��32 0 0 0q�Target Channel:�� -   $ TargetChannelText��$��8 0 0 0q�$
��None��$��Italic$��� $��32 0 0 0q�User:�� -   $ TargetUserText��$��8 0 0 0q�$
��None��$��Italic$��� $��32 0 0 0q�Pwd:�� -   $ TargetUserPassword��$��8 0 0 0q�$
��None��$��Italic$�