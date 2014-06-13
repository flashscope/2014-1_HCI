#include "ImageExample.h"

#include "..\MovingObject.h"
#include "..\Album.h"
#include "..\Photo.h"

//--------------------------------------------------------------

ofGLRenderer* g_OpenGLRenderer = nullptr;

void ImageExample::setup(){
	ofSetFrameRate(60);
	// Connect
	MyClient.connect(3333);

	ofAddListener(ofEvents().touchDown, this, &ImageExample::touchDown);
	ofAddListener(ofEvents().touchMoved, this, &ImageExample::touchMoved);
	ofAddListener(ofEvents().touchUp, this, &ImageExample::touchUp);



	m_MyImage.loadImage("C:\\Penguins.jpg");

	// 해제는 어디서 하죠?
	g_OpenGLRenderer = new ofGLRenderer();

	m_ImgPositionX = 100.0;
	m_ImgPositionY = 100.0;
	m_ImgWidth = 200.0;
	m_ImgHeight = 200.0;
	m_ImgRotateAngle = 120.0;

	
	//Photo* photo1 = new Photo("C:\\p02.jpg", 200, 200, 0 , 1);
	//photo1->SetVisible(true);
	//m_MovingObjectList.push_back( photo1 );

	m_MovingObjectList.push_back( new Photo("C:\\p01.jpg", 200, 200, 0 , 1) );
	m_MovingObjectList.push_back( new Photo("C:\\p02.jpg", 200, 600, 0 , 1) );

	m_MovingObjectList.push_back( new Photo("C:\\p03.jpg", 800, 200, 0 , 2) );
	m_MovingObjectList.push_back( new Photo("C:\\p04.jpg", 800, 600, 0 , 2) );


	Album* album1 = new Album("C:\\album.png", 200, 400, 0 , 1);
	Album* album2 = new Album("C:\\album.png", 800, 400, 0 , 2);
	m_MovingObjectList.push_back( album1 );
	m_MovingObjectList.push_back( album2 );
}

//--------------------------------------------------------------
void ImageExample::update(){

}

//--------------------------------------------------------------
void ImageExample::draw(){

	ofSetColor(255, 255, 255);
	for ( auto& it : m_MovingObjectList )
	{
		MovingObject* movingObject = it;
		movingObject->Draw();
	}


	//MyClient.drawCursors();
	//MyClient.drawObjects();

	ofSetColor(255, 0, 255);
	ofCircle(m_CursorCenterX, m_CursorCenterY, 10);

	

	/*
	g_OpenGLRenderer->pushMatrix();
		g_OpenGLRenderer->translate(m_ImgPositionX, m_ImgPositionY);
		g_OpenGLRenderer->rotateZ(m_ImgRotateAngle);

		g_OpenGLRenderer->draw(m_MyImage, -m_ImgWidth / 2, -m_ImgHeight / 2, 0.0,
				m_ImgWidth, m_ImgHeight,
				0.0, 0.0,
				m_MyImage.getWidth(), m_MyImage.getHeight());

	g_OpenGLRenderer->popMatrix();
	*/
}

//--------------------------------------------------------------
void ImageExample::keyPressed(int key){
}

//--------------------------------------------------------------
void ImageExample::keyReleased(int key){
}

//--------------------------------------------------------------
void ImageExample::mouseMoved(int x, int y){
}

//--------------------------------------------------------------
void ImageExample::mouseDragged(int x, int y, int button){
}

//--------------------------------------------------------------
void ImageExample::mousePressed(int x, int y, int button){
}

//--------------------------------------------------------------
void ImageExample::mouseReleased(int x, int y, int button){
}

//--------------------------------------------------------------
void ImageExample::windowResized(int w, int h){
}

//--------------------------------------------------------------
void ImageExample::gotMessage(ofMessage msg){
}

//--------------------------------------------------------------
void ImageExample::dragEvent(ofDragInfo dragInfo){ 
}


void ImageExample::touchDown( ofTouchEventArgs & touch )
{	
	CursorCenterCalc();
	
	int cursorSize = MyClient.client->getTuioCursors().size();

	if (cursorSize == -1)
	{
		return;
	}

	switch (cursorSize)
	{
	case 0:
		break;
	case 1:
		OneTouchDownEvent();
		break;
	case 2:
		TwoTouchDownEvent();
		break;
	case 3:
		ThreeTouchDownEvent();
		break;
	default:
		MultiTouchDownEvent();
		break;
	}



	printf_s("down \n");
}

void ImageExample::touchMoved( ofTouchEventArgs & touch )
{
	CursorCenterCalc();

	int cursorSize = MyClient.client->getTuioCursors().size();

	if (cursorSize == -1)
	{
		return;
	}

	switch (cursorSize)
	{
	case 0:
		break;
	case 1:
		OneTouchMoveEvent();
		break;
	case 2:
		TwoTouchMoveEvent();
		break;
	case 3:
		ThreeTouchMoveEvent();
		break;
	default:
		MultiTouchMoveEvent();
		break;
	}
	
}

void ImageExample::touchUp( ofTouchEventArgs & touch )
{
	m_TwoCursorLastDistance = -1;
	m_ThreeCursorLastAngle = -1;
	//m_MultiCursorLastDistance = -1;

}




//////////////////////////////////////////////////////////////////////////
//////////////////////////////////////////////////////////////////////////




void ImageExample::CursorCenterCalc()
{ 
	int minX = INT_MAX;
	int maxX = INT_MIN;
	int minY = INT_MAX;
	int maxY = INT_MIN;


	for ( auto *iter : MyClient.client->getTuioCursors() )
	{
		int x = iter->getScreenX(WINDOW_WIDTH);
		int y = iter->getScreenY(WINDOW_HEIGHT);

		minX = __min(x, minX);
		maxX = __max(x, maxX);
		minY = __min(y, minY);
		maxY = __max(y, maxY);

	}

	int centerX = ((maxX - minX)/2) + minX;
	int centerY = ((maxY - minY)/2) + minY;

	//printf_s("min:[%d][%d] max:[%d][%d] center:[%d][%d] \n", minX, minY, maxX, maxY, centerX, centerY);

	m_CursorCenterX = centerX;
	m_CursorCenterY = centerY;
}


float ImageExample::GetCursorBoundingBox()
{
	int minX = INT_MAX;
	int maxX = INT_MIN;
	int minY = INT_MAX;
	int maxY = INT_MIN;


	for ( auto *iter : MyClient.client->getTuioCursors() )
	{
		int x = iter->getScreenX(WINDOW_WIDTH);
		int y = iter->getScreenY(WINDOW_HEIGHT);

		minX = __min(x, minX);
		maxX = __max(x, maxX);
		minY = __min(y, minY);
		maxY = __max(y, maxY);

	}

	ofPoint p1(minX, minY);
	ofPoint p2(maxX, maxY);
	ofPoint delta = p2 - p1;
	float distBetween = sqrt(delta.x * delta.x + delta.y * delta.y);

	return distBetween;
}


void ImageExample::OneTouchDownEvent()
{
	int group = -1;
	int albumOpen = false;

	for ( auto& it : m_MovingObjectList )
	{
		
		MovingObject* movingObject = it;

		
		if ( movingObject->IsCollide( m_CursorCenterX, m_CursorCenterY ) )
		{
			if ( movingObject->GetMOType() == MO_TYPE_ALBUM )
			{
				//movingObject->AddSize(20);
				group = movingObject->GetGroup();
				albumOpen = movingObject->GetOpen();
				break;
			} 
			else if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				unsigned long long now = ofGetSeconds();
				unsigned long long last = movingObject->GetLastCheckTime();
				unsigned long long elapsedTime = now - last;
				if( elapsedTime < 2 )
				{
					movingObject->SetVisible(!movingObject->GetOpen());
					movingObject->SetOpen(!movingObject->GetOpen());
				}

				movingObject->SetCheckTime();
				group = -1;
				break;
			}
		}
		
	}

	if ( -1 != group )
	{
		for ( auto& it : m_MovingObjectList )
		{
			MovingObject* movingObject = it;
			if ( group == it->GetGroup() )
			{
				it->SetVisible(!albumOpen);
				movingObject->SetOpen(!albumOpen);
			}
		}
	}

}

void ImageExample::OneTouchMoveEvent()
{
	printf_s("OneTouchMoveEvent [%d][%d]\n", m_CursorCenterX, m_CursorCenterY);
	
	for ( auto& it : m_MovingObjectList )
	{
		MovingObject* movingObject = it;

		if ( movingObject->IsCollide( m_CursorCenterX, m_CursorCenterY ) )
		{
			if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				movingObject->SetPosition(m_CursorCenterX, m_CursorCenterY);
				break;
			}
		}
	}

}

void ImageExample::TwoTouchDownEvent()
{
	printf_s("TwoTouchDownEvent [%d][%d]\n", m_CursorCenterX, m_CursorCenterY);


	std::vector<TUIO::TuioCursor*> cursors;
	for ( auto *iter : MyClient.client->getTuioCursors() )
	{
		cursors.push_back( iter );
	}

	if (cursors.size() != 2 )
	{
		return;
	}

	TUIO::TuioCursor* cursor1 = cursors.at(0);
	TUIO::TuioCursor* cursor2 = cursors.at(1);
	float distance = cursor1->getDistance(cursor2->getScreenX(WINDOW_WIDTH), cursor2->getScreenY(WINDOW_HEIGHT));
	

	for ( auto& it : m_MovingObjectList )
	{
		MovingObject* movingObject = it;

		if ( movingObject->IsCollide( m_CursorCenterX, m_CursorCenterY ) )
		{
			if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				m_TwoCursorLastDistance = distance;
				break;
			}
		}
	}
}

void ImageExample::TwoTouchMoveEvent()
{


	std::vector<TUIO::TuioCursor*> cursors;
	for ( auto *iter : MyClient.client->getTuioCursors() )
	{
		cursors.push_back( iter );
	}

	if (cursors.size() != 2 )
	{
		return;
	}


	if (m_TwoCursorLastDistance == -1 )
	{
		return;
	}

	TUIO::TuioCursor* cursor1 = cursors.at(0);
	TUIO::TuioCursor* cursor2 = cursors.at(1);
	float distance = cursor1->getDistance(cursor2->getScreenX(WINDOW_WIDTH), cursor2->getScreenY(WINDOW_HEIGHT));




	for ( auto& it : m_MovingObjectList )
	{
		MovingObject* movingObject = it;

		if ( movingObject->IsCollide( m_CursorCenterX, m_CursorCenterY ) )
		{
			if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				float moveDistance = distance - m_TwoCursorLastDistance;

				movingObject->AddSize(moveDistance*1.5);
				m_TwoCursorLastDistance = distance;

				break;
			}
		}
	}
}

void ImageExample::ThreeTouchDownEvent()
{
	printf_s("ThreeTouchDownEvent [%d][%d]\n", m_CursorCenterX, m_CursorCenterY);


	std::vector<TUIO::TuioCursor*> cursors;
	for ( auto *iter : MyClient.client->getTuioCursors() )
	{
		cursors.push_back( iter );
	}

	if (cursors.size() != 3 )
	{
		return;
	}

	TUIO::TuioCursor* cursor1 = cursors.at(0);
	TUIO::TuioCursor* cursor2 = cursors.at(1);
	float angle = cursor1->getAngleDegrees(cursor2->getScreenX(WINDOW_WIDTH), cursor2->getScreenY(WINDOW_HEIGHT));

	for ( auto& it : m_MovingObjectList )
	{
		MovingObject* movingObject = it;

		if ( movingObject->IsCollide( m_CursorCenterX, m_CursorCenterY ) )
		{
			if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				m_ThreeCursorLastAngle = angle;
				break;
			}
		}
	}
}



void ImageExample::ThreeTouchMoveEvent()
{


	std::vector<TUIO::TuioCursor*> cursors;
	for ( auto *iter : MyClient.client->getTuioCursors() )
	{
		cursors.push_back( iter );
	}

	if (cursors.size() != 3 )
	{
		return;
	}


	if (m_ThreeCursorLastAngle == -1 )
	{
		return;
	}

	TUIO::TuioCursor* cursor1 = cursors.at(0);
	TUIO::TuioCursor* cursor2 = cursors.at(1);
	float angle = cursor1->getAngleDegrees(cursor2->getScreenX(WINDOW_WIDTH), cursor2->getScreenY(WINDOW_HEIGHT));




	for ( auto& it : m_MovingObjectList )
	{
		MovingObject* movingObject = it;

		if ( movingObject->IsCollide( m_CursorCenterX, m_CursorCenterY ) )
		{
			if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				float moveAngle = angle - m_ThreeCursorLastAngle;

				movingObject->AddAngle(-moveAngle*3);
				m_ThreeCursorLastAngle = angle;

				break;
			}
		}
	}
}

void ImageExample::MultiTouchDownEvent()
{
	printf_s("MultiTouchDownEvent [%d][%d]\n", m_CursorCenterX, m_CursorCenterY);

	if ( MyClient.client->getTuioCursors().size() < 4 )
	{
		return;
	}

	m_MultiCursorLastDistance = GetCursorBoundingBox();
	
}


bool inputResetOnce = true;
void ImageExample::MultiTouchMoveEvent()
{


	if ( MyClient.client->getTuioCursors().size() < 4 )
	{
		return;
	}

	

	if ( m_MultiCursorLastDistance == -1 )
	{
		return;
	}


	float distance = GetCursorBoundingBox();
	float moveDistance = m_MultiCursorLastDistance - distance;
	if ( moveDistance > 200 )
	{
		for ( auto& it : m_MovingObjectList )
		{
			MovingObject* movingObject = it;
			if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				if (inputResetOnce)
				{
					movingObject->SetMovePositionTemp();
					inputResetOnce = false;
				}
				
				movingObject->SetPosition((WINDOW_WIDTH/2) + (rand() % 100), (WINDOW_HEIGHT/2) + (rand() % 100));
			}
		}
		m_MultiCursorLastDistance = distance;
	} 


	else if ( moveDistance < -200 )
	{
		for ( auto& it : m_MovingObjectList )
		{
			MovingObject* movingObject = it;
			if ( movingObject->GetMOType() == MO_TYPE_PHOTO )
			{
				movingObject->MoveBackToTempPosition();
				m_MultiCursorLastDistance = -1;
				inputResetOnce = true;
			}
		}
		m_MultiCursorLastDistance = distance;
	}


}


