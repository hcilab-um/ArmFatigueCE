#pragma once

#include "ofMain.h"
#include <windows.h>
#include <ole2.h>
#include "NuiApi.h"
#include "ofSerial.h"
#include "ArmFatigueCE.h"

#define IMAGE_WIDTH 640
#define IMAGE_HEIGHT 480

class testApp : public ofBaseApp
{

private:
	FatigueEngine *engineCE;
	NUI_SKELETON_DATA skeleton;
	INuiSensor* pKinectSensor;

	ofTexture colorText;

	void ProcessColor();
	void ProcessSkeleton();

	void DrawSkeleton(const NUI_SKELETON_DATA & skel);

	void DrawBone(const NUI_SKELETON_DATA & skel, NUI_SKELETON_POSITION_INDEX joint0, NUI_SKELETON_POSITION_INDEX joint1);
	
	ofPoint SkeletonToScreen(Vector4 skeletonPoint, int width, int height);

public:
	HANDLE colorStreamHandle;
	HANDLE colorEvent;
	HANDLE skeletonEvent;

	NUI_LOCKED_RECT LockedRect;
	NUI_SKELETON_FRAME skeletonFrame;

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