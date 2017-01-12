----------------------
Application
----------------------
Name: BrainEReader
Author: WANG, Xi
website: http://gxtogether.wixsite.com/portfolio/brainereader

----------------------
Environmentment
----------------------
Software: 
Visual Studio 2013 and higher version.

Hardware:
NeuroSky Mindwave Mobile,
Internal notebook Bluetooth card or Bluetooth USB dongle.

System:
Windows 8 and higher version.

----------------------
Run Application
----------------------
1) Double click the shortcut in the current directory.
2) Double click PdfViewer.exe in BrainEReader\PdfViewer\bin\Debug.
3) Run BrainEReader\PdfViewer.sln in Visual Studio.

----------------------
Data
----------------------
Data is put in the BrainEReader\PdfViewer\bin\Debug\exp directory.
The EEG samples consisting of label, signal, raw data, frequency bands, attention and meditation.


This application has two modes: experiment mode and reading mode. Use the 'Settings' button to change modes.
For details about how to use this application, please see the demo video and steps-record.
----------------------
Experiment Mode
----------------------
In the experiment mode, editing the task and class name will change the file store path configuration.
Example: 
task: eye_states 
class: 1 
EEG sample files will be saved under the directory exp\eye_states\1. 

The name of a newly created EEG sample file is the same as the number of files in the directory, 
i.e., the EEG sample is named as 0 when the folder is empty and the second one is 1. 

Click the 'Start' button, an experiment starts. The EEG data will be recorded and stored in the path.
After 10 seconds, the experiment ends. The application records 1 EEG sample per 0.5 sec. One 10-sec experiment generates 20 samples.

Repeat performing experiments by re-clicking the 'Start' button.

In order to use this application, you need to collect your own brainwave data in at least 20 closing-eyes experiments and 20 opening-eyes experiments.
For closing-eyes experiment (class=1), just close eyes when experiment starts.
For opening-eyes experiment (class=2), keep eyes open and stare at the sentence on the pop-up information window.

Other instructions:
1) fit with MindWave Mobile in a sitting position in a quiet and closed room setting.
2) keep gaze steady on a point.
3) try not to blink eyes.
4) try not to move any part of your body. 

--------------------
Reading Mode
--------------------
In the reading mode, if you want to use your own data, put it in the exp\training_data folder.
closed-eyes data: exp\training_data\1
open-eyes data: exp\training_data\2

To activate turning page command:
Blink eyes twice strongly. If no window pops up, keep blinking.

To select command after seeing the pop up information window: 
Keep closing eyes --> turn to the previous page
Keep opening eyes --> turn to the next page

To confirm command:
Blink once in 3 sec -> cancel
Keep no blinking -> confirm

To turn on/off confirmation window and audio notification:
Change them in the 'Settings'. 
