   M S B A M L   `   `   `  ���� N  JNicoCommentTransfer, Version=1.4.0.1, Culture=neutral, PublicKeyToken=null9!clr-namespace:NicoCommentTransferNicoCommentTransfer  S.clr-namespace:NicoCommentTransfer.UserControls NicoCommentTransfer.UserControls  %    NicoCommentTransfer.LoginWindow   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     3local!clr-namespace:NicoCommentTransfer     Gusercontrols.clr-namespace:NicoCommentTransfer.UserControls     -     � Title$  Login��$	��230��$	��360�� � 
ResizeMode$ NoResize=� � 	Resources  �� 
TargetType   ImageButtonStyle �� Property �� Value �� 
TargetType��%:   &	        �� " ��.���  � �� �� " ��.R��� .��� .+��� -    ��Name$ LoginModeSelectCB��$
��Left=�$��	10,10,0,0q�$	��Top=�$	��200�� �� SelectedIndex$ 0��.@��� $#��Email/Pssswordでログイン���� $��Chromeでログイン���� $��Firefoxでログイン��� -   	 �Name$	 EmailLoginGrid������ " �.���  � y�$ 	Collapsed=�
 �� Triggers
 ��  �� Binding ��  �� ElementName$ LoginModeSelectCB�� �� Path$ SelectedIndex� �� Value 0.a���  � y�$ Visible=�.+��� $
��Left=�$��	10,37,0,0q� �� Padding$	 2,1q�$
��Wrap=�$��Email��$	��Top=�$��23��$��80��$��14(�/   )NicoCommentTransfer.UserControls.TextBoxV  -     Name$ 	EmailTBox��$
��Left=�$��23��$��	95,37,0,0q�$	��Top=�$	��238��$��14(�   BackText$ yellow@example.com���� $
��Left=�$��	10,65,0,0q�$	 2,1q�$
��Wrap=�$��Password��$	��Top=�$��23��$��80��$��14(�G� -    G�Name$ PassTBox��$
��Left=�$��23��$��	95,65,0,0q�$	��Top=�$	��238��$��14(��� -   $��Login��$
��Left=�$��
258,93,0,0q�$	��Top=�$��75��� -   $	 ChromeLoginGrid������ " �.���  � y�$ 	Collapsed=�
 ��  �� $ LoginModeSelectCB��$ SelectedIndex� 1.a���  � y�$ Visible=�.+��� -   $��ChromeからLogin��$
��Left=�$��	10,39,0,0q�$	��Top=�$	��323��$��75��$��14(�� -	   $	 FirefoxLoginGrid������ " �.���  � y�$ 	Collapsed=�
 ��  �� $ LoginModeSelectCB��$ SelectedIndex� 2.a���  � y�$ Visible=�.+��� -
   $��FirefoxからLogin��$
��Left=�$��	10,39,0,0q�$	��Top=�$	��323��$��75��$��14(��� -    ��Name$ OKBtn��$��OK��$��Right=�$��0,0,9.667,10q�$��Bottom=�$��75��{�.  �� $
��Left=�$	��Top=�$��
10,124,0,0q�$��50��$��50��
�������$��1q�._��� -   #��[  .���� -    ��Name$ 	UserImage���� -    ��Name$ 
UserNameTB��$
��Left=�$��
65,124,0,0q�$
��Wrap=�$
��Name��$	��Top=�$	��192���� -   $ UserIDTB��$
��Left=�$��
65,140,0,0q�$
��Wrap=�$��ID��$	��Top=�$��97���� -   $ SessionExpiresTB��$
��Left=�$��
65,156,0,0q�$
��Wrap=�$��Expires��$	��Top=�$	��192���� -   $ UserPremium��$
��Left=�$��167,140,0,0q�$
��Wrap=�$��Premium��$	��Top=�$��90��