using Microsoft.EntityFrameworkCore;
using SPCoEdit.Database;
using SPCoEdit.Dto.CoEdit;

namespace SPCoEdit.Utils
{
    public class DbUtils(AppDbContext _context)
    {
        public Task<List<IdleSessionGroup>> GetIdleSessionGroupsAsync(
            DateTime cutoff, CancellationToken cancellationToken)
        {
            // Filter after grouping: a newer session must keep the entire node active.
            return _context.SPCoEditSessions
                .AsNoTracking()
                .GroupBy(session => session.NodeID)
                .Select(group => new IdleSessionGroup
                {
                    WebUrl = group.Max(session => session.WebUrl),
                    NodeID = group.Key,
                    SessionCount = group.Count(),
                    LatestSessionCreatedAt = group.Max(session => session.CreatedAt)
                })
                .Where(group => group.LatestSessionCreatedAt <= cutoff && group.WebUrl != null)
                .OrderBy(group => group.NodeID)
                .ToListAsync(cancellationToken);
        }

        public DTreeCore? GetNode(long nodeId)
        {
            return _context.DTreeCore.FirstOrDefault(x => x.DataID == nodeId);
        }
        public void UpdateSessionWebUrl(long ID, string webUrl)
        {
            var session = _context.SPCoEditSessions.FirstOrDefault(s => s.ID == ID);
            if (session != null)
            {
                session.WebUrl = webUrl;
                _context.SaveChanges();
            }
        }
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
