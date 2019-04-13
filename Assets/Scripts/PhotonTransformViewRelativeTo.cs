using UnityEngine;


namespace Photon.Pun
{
    public class PhotonTransformViewRelativeTo : MonoBehaviour, IPunObservable
    {
        public string OriginGameObjectName;
        public GameObject origin;

        private float m_Distance;
        private float m_Angle;

        private PhotonView m_PhotonView;

        private Vector3 m_Direction; // used to interpolate for lag
        private Vector3 m_NetworkPosition;
        private Vector3 m_StoredPosition;

        private Quaternion m_NetworkRotation;

        public bool m_SynchronizePosition = true;
        public bool m_SynchronizeRotation = true;
        public bool m_SynchronizeScale = false;

        public void Awake()
        {
            origin = GameObject.Find(OriginGameObjectName);

            m_PhotonView = GetComponent<PhotonView>();

            //m_StoredPosition = transform.position;
            Vector3 relPosition = transform.position - origin.transform.position;
            m_StoredPosition = relPosition;
            m_NetworkPosition = Vector3.zero;

            m_NetworkRotation = Quaternion.identity;
        }

        public void Update()
        {
            if (origin == null)
            {
                origin = GameObject.Find(OriginGameObjectName);
            }

            if (!m_PhotonView.IsMine)
            {
                transform.position = Vector3.MoveTowards(transform.position, m_NetworkPosition, m_Distance * (1.0f / PhotonNetwork.SerializationRate));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, m_NetworkRotation, m_Angle * (1.0f / PhotonNetwork.SerializationRate));
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                if (m_SynchronizePosition)
                {
                    Vector3 originPos = origin == null ? new Vector3(0, 0, 0) : origin.transform.position;

                    Vector3 relPosition = transform.position - originPos;

                    //m_Direction = transform.position - m_StoredPosition;
                    m_Direction = relPosition - m_StoredPosition;
                    //m_StoredPosition = transform.position;
                    m_StoredPosition = relPosition; // store, use as last

                    //stream.SendNext(transform.position);
                    stream.SendNext(relPosition);
                    stream.SendNext(m_Direction);
                }

                if (m_SynchronizeRotation)
                {
                    stream.SendNext(transform.rotation);
                }

                if (m_SynchronizeScale)
                {
                    stream.SendNext(transform.localScale);
                }
            }
            else
            {
                if (m_SynchronizePosition)
                {
                    Vector3 originPos = origin == null ? new Vector3(0, 0, 0) : origin.transform.position;

                    //m_NetworkPosition = (Vector3)stream.ReceiveNext();
                    m_NetworkPosition = originPos + (Vector3)stream.ReceiveNext();
                    m_Direction = (Vector3)stream.ReceiveNext();

                    float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                    m_NetworkPosition += m_Direction * lag;

                    m_Distance = Vector3.Distance(transform.position, m_NetworkPosition);
                }

                if (m_SynchronizeRotation)
                {
                    m_NetworkRotation = (Quaternion)stream.ReceiveNext();

                    m_Angle = Quaternion.Angle(transform.rotation, m_NetworkRotation);
                }

                if (m_SynchronizeScale)
                {
                    transform.localScale = (Vector3)stream.ReceiveNext();
                }
            }
        }
    }
}