   M S B A M L   `   `   `  ����N  JNicoCommentTransfer, Version=1.4.0.1, Culture=neutral, PublicKeyToken=null9!clr-namespace:NicoCommentTransferNicoCommentTransfer  5   	   S.clr-namespace:NicoCommentTransfer.UserControls NicoCommentTransfer.UserControls  69   %    NicoCommentTransfer.LoginWindow   5   	   W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:NicoCommentTransfer     5   	   Gusercontrols.clr-namespace:NicoCommentTransfer.UserControls     69   -   5          � Title$  Login��5   	   $	��230��6   $	��360��6$    � 
ResizeMode$ NoResize=�60    � 	Resources 5	       �� 
TargetType   ImageButtonStyle �� Property �� Value �� 
TargetType��%�   &	        �� 5
   
   " ��6   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      �� 5      5      5      5      .��5      � .+�5   
   �� -   5         ��Name$ LoginModeSelectCB��5      $
��Left=�6.   $��	10,10,0,0q�6I   $	��Top=�6\   $	��200��6t    �� SelectedIndex$ 0��6�   .@�5      �� $#��Email/Pssswordでログイン��6   �� 5      $��Chromeでログイン��6   �� 5      $��Firefoxでログイン��6   5      � 5   
   -   5        	 �Name$	 EmailLoginGrid��5      ��5      �� 5      " �6   .�5      ��  � y�$ 	Collapsed=�63   
 �� Triggers
 5      �� 5       �� Binding 6&   ��  �� ElementName$ LoginModeSelectCB�� �� Path$ SelectedIndex� �� Value 06l   .a�5       ��  � y�$ Visible=�6;   5!      5#      .+�5%      �� $
��Left=�6   $��	10,37,0,0q�63    �� Padding$	 2,1q�6F   $
��Wrap=�6T   $��Email��6h   $	��Top=�6u   $��23��6�   $��80��6�   $��14(�6�   /   )NicoCommentTransfer.UserControls.TextBoxV  5&      -   5          Name$ 	EmailTBox��5&   $   $
��Left=�67   $��23��6R   $��	95,37,0,0q�6^   $	��Top=�6q   $	��238��6�   $��14(�6�      BackText$ yellow@example.com��6�   �� 5'      $
��Left=�6   $��	10,65,0,0q�63   $	 2,1q�6F   $
��Wrap=�6T   $��Password��6h   $	��Top=�6x   $��23��6�   $��80��6�   $��14(�6�   G� 5(      -   5         G�Name$ PassTBox��5(      $
��Left=�6,   $��23��6G   $��	95,65,0,0q�6S   $	��Top=�6f   $	��238��6~   $��14(�6�   �� 5)      -   5        $��Login��5)   0   $
��Left=�6@   $��
258,93,0,0q�6[   $	��Top=�6o   $��75��6�   5*      � 5+   
   -   5        $	 ChromeLoginGrid��5+      ��5,      �� 5-      " �6   .�5.      ��  � y�$ 	Collapsed=�63   
 5/      �� 50       6&   �� $ LoginModeSelectCB��$ SelectedIndex� 16l   .a�51      ��  � y�$ Visible=�6;   52      54      .+�56      �� -   5        $��ChromeからLogin��56   1   $
��Left=�6I   $��	10,39,0,0q�6d   $	��Top=�6w   $	��323��6�   $��75��6�   $��14(�6�   57      � 58   
   -	   5        $	 FirefoxLoginGrid��58      ��59      �� 5:      " �6   .�5;      ��  � y�$ 	Collapsed=�63   
 5<      �� 5=       6&   �� $ LoginModeSelectCB��$ SelectedIndex� 26l   .a�5>      ��  � y�$ Visible=�6;   5?      5A      .+�5C      �� -
   5        $��FirefoxからLogin��5C   2   $
��Left=�6K   $��	10,39,0,0q�6f   $	��Top=�6y   $	��323��6�   $��75��6�   $��14(�6�   5D      �� 5E   
   -   5         ��Name$ OKBtn��5E      $��OK��6    $��Right=�6-   $��0,0,9.667,10q�6I   $��Bottom=�6_   $��75��6z   {�.  6�   �� 5F   
   $
��Left=�6   $	��Top=�6,   $��
10,124,0,0q�6D   $��50��6X   $��50��6c   
�������$��1q�6�   ._�5G      �� -   5        #��[  5G      .��5H      �� -   5         ��Name$ 	UserImage��5H      5I      5J      �� 5K   
   -   5         ��Name$ 
UserNameTB��5K      $
��Left=�6(   $��
65,124,0,0q�6C   $
��Wrap=�6W   $
��Name��6k   $	��Top=�6w   $	��192��6�   �� 5L   
   -   5        $ UserIDTB��5L      $
��Left=�6&   $��
65,140,0,0q�6A   $
��Wrap=�6U   $��ID��6i   $	��Top=�6s   $��97��6�   �� 5M   
   -   5        $ SessionExpiresTB��5M      $
��Left=�6.   $��
65,156,0,0q�6I   $
��Wrap=�6]   $��Expires��6q   $	��Top=�6�   $	��192��6�   �� 5N   
   -   5        $ UserPremium��5N      $
��Left=�6)   $��167,140,0,0q�6D   $
��Wrap=�6Y   $��Premium��6m   $	��Top=�6|   $��90��6�   5O      5P      