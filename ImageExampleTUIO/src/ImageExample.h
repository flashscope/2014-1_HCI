#pragma once

#include "ofMain.h"
#include "ofxTuioClient.h"
// for openGL
#include "ofGLRenderer.h"

#define WINDOW_WIDTH 1024
#define WINDOW_HEIGHT 768

class MovingObject;
class ImageExample : public ofBaseApp {
	public:
		void setup();
		void update();
		void draw();
		
		void keyPressed(int key);
		void keyReleased(int key);
		void mouseMoved(int x, int y);
		void mouseDragged(int x, int y, int button);
		void mousePressed(int x, int y, int button);
		void mouseReleased(int x, int y, int button);
		void windowResized(int w, int h);
		void dragEvent(ofDragInfo dragInfo);
		void gotMessage(ofMessage msg);

	// added to Example
		ofxTuioClient MyClient;

		void touchDown(ofTouchEventArgs & touch);
		void touchMoved(ofTouchEventArgs & touch);
		void touchUp(ofTouchEventArgs & touch);

private:
	void	CursorCenterCalc();
	float	GetCursorBoundingBox();

	void	OneTouchDownEvent();
	void	OneTouchMoveEvent();

	void	TwoTouchDownEvent();
	void	TwoTouchMoveEvent();

	void	ThreeTouchDownEvent();
	void	ThreeTouchMoveEvent();

	void	MultiTouchDownEvent();
	void	MultiTouchMoveEvent();
private:
	std::vector<MovingObject*> m_MovingObjectList;

	ofImage m_MyImage;

	float m_ImgPositionX;
	float m_ImgPositionY;
	float m_ImgWidth;
	float m_ImgHeight;
	float m_ImgRotateAngle;




	int		m_CursorCenterX;
	int		m_CursorCenterY;

	float	m_TwoCursorLastDistance;
	float	m_ThreeCursorLastAngle;
	float	m_MultiCursorLastDistance;
};
extern ofGLRenderer*	g_OpenGLRenderer;