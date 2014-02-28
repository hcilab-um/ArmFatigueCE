#pragma once

#include "ofMain.h"
#include <windows.h>
#include <ole2.h>
#include "NuiApi.h"
#include "ofSerial.h"
#include "ofAppGlutWindow.h"
#include "ArmFatigueCE.h"
#include "ofxGui.h"

#define IMAGE_WIDTH 640
#define IMAGE_HEIGHT 480

#define WINDOW_WIDTH IMAGE_WIDTH
#define WINDOW_HEIGHT 530

#define SKELETON_STROKE 5

class testApp : public ofBaseApp{

private:
	
	ofAppGlutWindow *window;
	
	HANDLE colorStreamHandle;
	HANDLE colorEvent;
	HANDLE skeletonEvent;

	NUI_LOCKED_RECT LockedRect;
	NUI_SKELETON_FRAME skeletonFrame;

	INuiSensor* pKinectSensor;
	NUI_SKELETON_DATA skeleton;
	FatigueEngine *engineCE;
	bool startEngine;
	ofTexture colorText;

	long lastUpdate;

	HRESULT startKinect();
	void stopKinect();

	void processColor();
	void processSkeleton();

	void drawSkeleton(const NUI_SKELETON_DATA & skel);

	void drawBone(const NUI_SKELETON_DATA & skel, NUI_SKELETON_POSITION_INDEX joint0, NUI_SKELETON_POSITION_INDEX joint1);
	
	Vector3D convert(Vector4 trackedPoint);

	ofPoint skeletonToScreen(Vector4 skeletonPoint, int width, int height);

public:
	ofxButton startButton;
	ofxButton stopButton;
	ofxLabel labelCE;
	ofxPanel gui;
	
	testApp(ofAppGlutWindow * window);
	
	void startButtonClick();
	void stopButtonClick();

	void setup();
	void update();
	void draw();
	void exit();

	void keyPressed  (int key);
	void keyReleased(int key);
	void mouseMoved(int x, int y );
	void mouseDragged(int x, int y, int button);
	void mousePressed(int x, int y, int button);
	void mouseReleased(int x, int y, int button);
	void windowResized(int w, int h);
	void dragEvent(ofDragInfo dragInfo);
	void gotMessage(ofMessage msg);
};