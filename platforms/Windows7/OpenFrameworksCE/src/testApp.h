#pragma once

#include "ofMain.h"

#include <windows.h>
#include <ole2.h>
#include "NuiApi.h"
#include "ofSerial.h"

class testApp : public ofBaseApp
{

private:

	ofTexture colorText;

	INuiSensor* pKinectSensor;
	ofPoint getRightHand();
	void getSkeleton(float* x, float* y);
  void ProcessColor();

public:
	HANDLE colorStreamHandle;
	HANDLE colorEvent;
	HANDLE skeletonEvent;
	const NUI_IMAGE_FRAME* image;
	INuiFrameTexture* pTexture;
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
