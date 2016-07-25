using System.ComponentModel;

namespace DCache
{
    public partial class Instance
    {
        private BackgroundWorker m_StabilizeSuccessors = new BackgroundWorker();
        private BackgroundWorker m_StabilizePredecessors = new BackgroundWorker();
        private BackgroundWorker m_UpdateFingerTable = new BackgroundWorker();

        private void StartMaintenance()
        {
            m_StabilizeSuccessors.DoWork += new DoWorkEventHandler(this.StabilizeSuccessors);
            m_StabilizeSuccessors.WorkerSupportsCancellation = true;
            m_StabilizeSuccessors.RunWorkerAsync();

            m_StabilizePredecessors.DoWork += new DoWorkEventHandler(this.StabilizePredecessors);
            m_StabilizePredecessors.WorkerSupportsCancellation = true;
            m_StabilizePredecessors.RunWorkerAsync();
        }

        private void StopMaintenance()
        {
            m_StabilizeSuccessors.CancelAsync();
            m_StabilizePredecessors.CancelAsync();
            m_UpdateFingerTable.CancelAsync();
        }
    }
}
