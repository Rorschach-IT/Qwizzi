import { useEffect, useState } from 'react';
import { ArrowDownUp, Check, LoaderCircle, Pencil, RefreshCw, Trash2, UserRound, UserPlus, X } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ApiError } from '../../../core/api/apiError';
import { useAuth } from '../../auth/context/useAuth';
import {
  adminUsersService,
  type AdminUser,
  type UserSortField,
} from '../api/adminUsersService';

const editUserSchema = z.object({
  username: z.string().trim().min(3).max(50).regex(/^[A-Za-z0-9_-]+$/),
  email: z.string().trim().email().max(255),
  password: z.union([
    z.literal(''),
    z.string().min(8).max(64)
      .regex(/[a-z]/)
      .regex(/[A-Z]/)
      .regex(/\d/)
      .regex(/[\W_]/),
  ]),
});

type EditUserValues = z.infer<typeof editUserSchema>;

function formatDate(value: string) {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? '—'
    : new Intl.DateTimeFormat('en', { dateStyle: 'medium' }).format(date);
}

export function AdminUsersPage() {
  const { user: currentUser } = useAuth();
  const [users, setUsers] = useState<AdminUser[]>([]);
  const [loadError, setLoadError] = useState('');
  const [actionError, setActionError] = useState('');
  const [sortBy, setSortBy] = useState<UserSortField>('username');
  const [sortOrder, setSortOrder] = useState<'asc' | 'desc'>('asc');
  const [requestVersion, setRequestVersion] = useState(0);
  const [completedRequest, setCompletedRequest] = useState('');
  const [editingUser, setEditingUser] = useState<AdminUser | null>(null);
  const [creatingUser, setCreatingUser] = useState(false);
  const [saving, setSaving] = useState(false);
  const requestKey = `${sortBy}-${sortOrder}-${requestVersion}`;
  const loading = completedRequest !== requestKey;
  const editForm = useForm<EditUserValues>({
    resolver: zodResolver(editUserSchema),
    defaultValues: { username: '', email: '', password: '' },
  });

  useEffect(() => {
    let active = true;
    void adminUsersService
      .list(sortBy, sortOrder)
      .then((loadedUsers) => {
        if (!active) return;
        setUsers(loadedUsers);
        setLoadError('');
      })
      .catch((error: unknown) => {
        if (!active) return;
        setLoadError(
          error instanceof ApiError
            ? error.message
            : 'Unable to load users. Check your connection and try again.',
        );
      })
      .finally(() => {
        if (active) setCompletedRequest(requestKey);
      });

    return () => {
      active = false;
    };
  }, [sortBy, sortOrder, requestKey]);

  function reloadUsers() {
    setLoadError('');
    setRequestVersion((version) => version + 1);
  }

  function startEditing(user: AdminUser) {
    setCreatingUser(false);
    setEditingUser(user);
    setActionError('');
    editForm.reset({ username: user.username, email: user.email, password: '' });
  }

  function startCreating() {
    setCreatingUser(true);
    setEditingUser(null);
    setActionError('');
    editForm.reset({ username: '', email: '', password: '' });
  }

  async function saveUser(values: EditUserValues) {
    if (!editingUser && !creatingUser) return;
    setSaving(true);
    setActionError('');
    let updated = false;
    try {
      if (creatingUser) {
        if (!values.password) {
          setActionError('Set a temporary password for the new account.');
          return;
        }
        await adminUsersService.create({
          username: values.username,
          email: values.email,
          temporaryPassword: values.password,
        });
        setCreatingUser(false);
        reloadUsers();
        return;
      }
      if (!editingUser) return;
      if (values.username !== editingUser.username) {
        await adminUsersService.updateUsername(editingUser.userId, values.username);
        updated = true;
      }
      if (values.email !== editingUser.email) {
        await adminUsersService.updateEmail(editingUser.userId, values.email);
        updated = true;
      }
      if (values.password) {
        await adminUsersService.setPassword(editingUser.userId, values.password);
        updated = true;
      }
      setEditingUser(null);
      reloadUsers();
    } catch (error) {
      if (updated) {
        setEditingUser(null);
        reloadUsers();
      }
      setActionError(
        error instanceof Error ? error.message : 'Unable to update this user.',
      );
    } finally {
      setSaving(false);
    }
  }

  async function toggleStatus(user: AdminUser) {
    setActionError('');
    try {
      await adminUsersService.setStatus(user.userId, !user.status);
      reloadUsers();
    } catch (error) {
      setActionError(
        error instanceof Error ? error.message : 'Unable to update user status.',
      );
    }
  }

  async function deleteUser(user: AdminUser) {
    if (!window.confirm(`Permanently delete ${user.username}?`)) return;
    setActionError('');
    try {
      await adminUsersService.delete(user.userId);
      reloadUsers();
    } catch (error) {
      setActionError(
        error instanceof Error ? error.message : 'Unable to delete this user.',
      );
    }
  }

  return (
    <section className="page-container admin-page">
      <div className="page-heading page-heading-row">
        <div>
          <span className="eyebrow">ADMINISTRATION</span>
          <h1>Manage users</h1>
          <p>Review accounts, update details, and manage access.</p>
        </div>
        <div className="admin-heading-actions">
          <div className="user-count"><UserRound size={17} aria-hidden="true" /> {users.length} users</div>
          <button type="button" className="button button-primary" onClick={startCreating}><UserPlus size={17} aria-hidden="true" /> Add user</button>
        </div>
      </div>

      <section className="table-card" aria-labelledby="users-table-heading">
        <div className="table-toolbar">
          <div><h2 id="users-table-heading">All users</h2><p>Account information and access status.</p></div>
          <div className="table-controls">
            <label className="sort-control">
              <span className="sr-only">Sort users by</span>
              <ArrowDownUp size={16} aria-hidden="true" />
              <select value={sortBy} onChange={(event) => {
                setLoadError('');
                setSortBy(event.target.value as UserSortField);
              }}>
                <option value="username">Username</option>
                <option value="status">Status</option>
                <option value="role">Role</option>
                <option value="createdAt">Date joined</option>
              </select>
            </label>
            <button
              type="button"
              className="icon-button refresh-button"
              aria-label="Refresh users"
              onClick={reloadUsers}
              disabled={loading}
            >
              <RefreshCw size={17} aria-hidden="true" />
            </button>
          </div>
        </div>
        <div className="table-sort-order">
          <button type="button" className="text-button" onClick={() => {
            setLoadError('');
            setSortOrder((order) => order === 'asc' ? 'desc' : 'asc');
          }}>
            Sort {sortOrder === 'asc' ? 'ascending' : 'descending'}
          </button>
        </div>

        {actionError && <div className="notice error-notice table-notice" role="alert">{actionError}</div>}
        {loadError ? (
          <div className="table-state table-error-state" role="alert">
            <p>{loadError}</p>
            <button className="button button-outline" type="button" onClick={reloadUsers}>Try again</button>
          </div>
        ) : loading ? (
          <div className="table-state" role="status"><LoaderCircle className="spin" size={23} /> Loading users…</div>
        ) : users.length === 0 ? (
          <div className="table-state">
            <span className="collection-state-icon"><UserRound size={23} /></span>
            <strong>No users found.</strong>
            <p>There are no accounts to display right now.</p>
          </div>
        ) : (
          <div className="table-scroll">
            <table className="users-table">
              <thead>
                <tr>
                  <th scope="col">User</th>
                  <th scope="col">Role</th>
                  <th scope="col">Status</th>
                  <th scope="col">Joined</th>
                  <th scope="col"><span className="sr-only">Actions</span></th>
                </tr>
              </thead>
              <tbody>
                {users.map((user) => (
                  <tr key={user.userId}>
                    <td>
                      <div className="table-user">
                        <span className="table-avatar">{user.username.slice(0, 1).toUpperCase()}</span>
                        <span><strong>{user.username}</strong><small>{user.email}</small></span>
                      </div>
                    </td>
                    <td><span className={`role-pill${user.role === 'Administrator' ? ' admin-role' : ''}`}>{user.role}</span></td>
                    <td><span className={`status-pill${user.status ? ' status-active' : ' status-inactive'}`}><i />{user.status ? 'Active' : 'Inactive'}</span></td>
                    <td className="date-cell">{formatDate(user.createdAt)}</td>
                    <td>
                      <div className="row-actions">
                        <button type="button" className="icon-button" aria-label={`Edit ${user.username}`} onClick={() => startEditing(user)}><Pencil size={16} /></button>
                        <button type="button" className={`status-action${user.status ? ' deactivate-action' : ''}`} onClick={() => void toggleStatus(user)} disabled={user.userId === currentUser?.userId}>{user.status ? 'Deactivate' : 'Activate'}</button>
                        <button type="button" className="icon-button danger-icon-button" aria-label={`Delete ${user.username}`} onClick={() => void deleteUser(user)} disabled={user.userId === currentUser?.userId}><Trash2 size={16} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {(editingUser || creatingUser) && (
        <div className="modal-backdrop" onMouseDown={() => { setEditingUser(null); setCreatingUser(false); }}>
          <section
            className="edit-user-modal"
            role="dialog"
            aria-modal="true"
            aria-labelledby="edit-user-title"
            onMouseDown={(event) => event.stopPropagation()}
          >
            <div className="modal-heading">
              <div><span className="eyebrow">USER ACCOUNT</span><h2 id="edit-user-title">{creatingUser ? 'Create a user' : `Edit ${editingUser?.username}`}</h2></div>
              <button type="button" className="icon-button" aria-label="Close user dialog" onClick={() => { setEditingUser(null); setCreatingUser(false); }}><X size={20} /></button>
            </div>
            {actionError && <div className="notice error-notice" role="alert">{actionError}</div>}
            <form className="settings-form" noValidate onSubmit={editForm.handleSubmit(saveUser)}>
              <div className="form-field"><label htmlFor="edit-username">Username</label><input id="edit-username" aria-invalid={Boolean(editForm.formState.errors.username)} aria-describedby={editForm.formState.errors.username ? 'edit-username-error' : undefined} {...editForm.register('username')} />{editForm.formState.errors.username && <p className="field-error" id="edit-username-error">Use 3–50 letters, numbers, underscores, or hyphens.</p>}</div>
              <div className="form-field"><label htmlFor="edit-email">Email address</label><input id="edit-email" type="email" aria-invalid={Boolean(editForm.formState.errors.email)} aria-describedby={editForm.formState.errors.email ? 'edit-email-error' : undefined} {...editForm.register('email')} />{editForm.formState.errors.email && <p className="field-error" id="edit-email-error">Enter a valid email address (maximum 255 characters).</p>}</div>
              <div className="form-field"><label htmlFor="edit-password">{creatingUser ? 'Temporary password' : 'Set a new password'} {!creatingUser && <span className="optional-label">Optional</span>}</label><input id="edit-password" type="password" autoComplete="new-password" aria-invalid={Boolean(editForm.formState.errors.password)} aria-describedby={editForm.formState.errors.password ? 'edit-password-error' : undefined} {...editForm.register('password')} />{editForm.formState.errors.password && <p className="field-error" id="edit-password-error">Use 8–64 characters with uppercase, lowercase, a number, and a special character.</p>}{creatingUser && <p className="field-hint">The user will be asked to change this password at first sign-in.</p>}</div>
              <div className="modal-actions">
                <button type="button" className="button button-outline" onClick={() => { setEditingUser(null); setCreatingUser(false); }}>Cancel</button>
                <button type="submit" className="button button-primary" disabled={saving}>{saving ? 'Saving…' : <><Check size={16} /> {creatingUser ? 'Create account' : 'Save changes'}</>}</button>
              </div>
            </form>
          </section>
        </div>
      )}
    </section>
  );
}
