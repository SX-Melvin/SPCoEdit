using Microsoft.EntityFrameworkCore;
using SPCoEdit.Database;

namespace SPCoEdit.Utils
{
    public class DbUtils(AppDbContext _context)
    {
        public SPCoEditSessions InsertSession(long nodeId)
        {
            var newSession = new SPCoEditSessions
            {
                CreatedAt = DateTime.Now,
                NodeID = nodeId
            };
            _context.SPCoEditSessions.Add(newSession);
            _context.SaveChanges();

            return newSession;
        }
        public void DeleteSession(long nodeID)
        {
            var existingSession = _context.SPCoEditSessions.FirstOrDefault(s => s.NodeID == nodeID);
            if (existingSession != null)
            {
                _context.SPCoEditSessions.Remove(existingSession);
            }
            _context.SaveChanges();
        }
        public bool IsSessionEmpty(long nodeId)
        {
            var existingSession = _context.SPCoEditSessions.FirstOrDefault(s => s.NodeID == nodeId);
            return existingSession == null;
        }
        public SPCoEditSessions? GetSession(long ID)
        {
            return _context.SPCoEditSessions.FirstOrDefault(s => s.ID == ID);
        }
    }
}
