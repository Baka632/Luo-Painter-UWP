//������Ҫ��������ͼ
#define D2D_INPUT_COUNT 3
//ԭͼ�������Ϊ��ģʽ
#define D2D_INPUT0_SAMPLE
//ѡ���������Ϊ��ģʽ
#define D2D_INPUT1_SAMPLE
//Ч��ͼ�������Ϊ��ģʽ
#define D2D_INPUT2_SAMPLE
#include "d2d1effecthelpers.hlsli"


D2D_PS_ENTRY(main) {
//��ȡѡ����ǰ���ص���ɫ
float4 area = D2DGetInput(0);
//�ж�ѡ���Ƿ�����ɫ��������򷵻�Ч��ͼ���أ����򷵻�ԭͼ����
if(area.a>0){
return D2DGetInput(1);
}
return D2DGetInput(2);

}