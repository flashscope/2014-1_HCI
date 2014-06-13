#include "MovingObject.h"
#include "src\ImageExample.h"

MovingObject::MovingObject( std::string imagePath, float positionX, float positionY, float angle, int group )
{
	m_MyImage1.loadImage(imagePath);
	m_PositionX = positionX;
	m_PositionY = positionY;

	m_SizeWidth = m_MyImage1.getWidth();
	m_SizeHeight = m_MyImage1.getHeight();

	m_Angle = angle;

	m_Group = group;
	m_IsVisible = false;
	m_Open = false;


	m_LastCheckTime = ofGetSeconds();


	m_PositionXTemp = positionX;
	m_PositionYTemp = positionY;
}


MovingObject::~MovingObject(void)
{
}






void MovingObject::Draw()
{
	if (m_IsVisible)
	{
		g_OpenGLRenderer->pushMatrix();
		g_OpenGLRenderer->translate(m_PositionX, m_PositionY);
		g_OpenGLRenderer->rotateZ(m_Angle);

		g_OpenGLRenderer->draw(m_MyImage1, -m_SizeWidth / 2, -m_SizeHeight / 2, 0.0,
			m_SizeWidth, m_SizeHeight,
			0.0, 0.0,
			m_MyImage1.getWidth(), m_MyImage1.getHeight());

		g_OpenGLRenderer->popMatrix();
	}


	/*
	if (m_ChildVisible)
	{
		for ( auto& it : m_MovingObjectChildList )
		{
			MovingObject* movingObject = it;
			it->Draw();
		}
	}
	*/
}



bool MovingObject::IsCollide( int x, int y )
{

	if (!m_IsVisible)
	{
		return false;
	}
	float minX = m_PositionX - (m_SizeWidth / 2);
	float minY = m_PositionY - (m_SizeHeight / 2);
	float maxX = m_PositionX + (m_SizeWidth / 2);
	float maxY = m_PositionY + (m_SizeHeight / 2);

	if (x < maxX && x > minX)
	{
		if (y < maxY && y > minY)
		{
			return true;
		}
	}

	return false;
}
