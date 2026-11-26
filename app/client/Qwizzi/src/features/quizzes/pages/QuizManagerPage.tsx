import { useEffect, useState } from 'react';
import { AlertCircle, Check, CirclePlus, ImagePlus, LoaderCircle, PencilLine, Plus, Trash2, X } from 'lucide-react';
import { ApiError } from '../../../core/api/apiError';
import { ImageCropDialog } from '../components/ImageCropDialog';
import {
  imageUrl,
  quizzesService,
  type ImageAsset,
  type QuizCategory,
  type QuizEditor,
  type QuizQuestion,
  type QuizSummary,
  type QuizWriteRequest,
} from '../api/quizzesService';

interface AnswerDraft {
  text: string;
  imageId: string | null;
  isCorrect: boolean;
}

interface QuestionDraft {
  text: string;
  imageId: string | null;
  explanation: string;
  answers: AnswerDraft[];
}

interface QuizDraft {
  title: string;
  categoryId: number;
  description: string;
  coverImageId: string | null;
  questions: QuestionDraft[];
}

type CropTarget = { kind: 'cover' } | { kind: 'question'; questionIndex: number } | { kind: 'answer'; questionIndex: number; answerIndex: number };

const blankQuestion = (): QuestionDraft => ({
  text: '',
  imageId: null,
  explanation: '',
  answers: [{ text: '', imageId: null, isCorrect: true }, { text: '', imageId: null, isCorrect: false }],
});

const blankQuiz = (categoryId = 0): QuizDraft => ({
  title: '',
  categoryId,
  description: '',
  coverImageId: null,
  questions: [blankQuestion()],
});

const statusNames: Record<number, string> = {
  1: 'Draft',
  2: 'Awaiting review',
  3: 'Approved',
  4: 'Changes requested',
};

function toDraft(editor: QuizEditor): QuizDraft {
  return {
    title: editor.title,
    categoryId: editor.categoryId,
    description: editor.description ?? '',
    coverImageId: editor.coverImageId,
    questions: editor.questions.map((question: QuizQuestion) => ({
      text: question.text,
      imageId: question.imageId,
      explanation: question.explanation ?? '',
      answers: question.answers.map((answer) => ({
        text: answer.text ?? '',
        imageId: answer.imageId,
        isCorrect: Boolean(answer.isCorrect),
      })),
    })),
  };
}

export function QuizManagerPage() {
  const [quizzes, setQuizzes] = useState<QuizSummary[]>([]);
  const [categories, setCategories] = useState<QuizCategory[]>([]);
  const [draft, setDraft] = useState<QuizDraft | null>(null);
  const [editingQuizId, setEditingQuizId] = useState<string | null>(null);
  const [cropTarget, setCropTarget] = useState<CropTarget | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [notice, setNotice] = useState('');

  async function loadData(showLoading = true) {
    if (showLoading) setLoading(true);
    try {
      const [mine, availableCategories] = await Promise.all([quizzesService.mine(), quizzesService.categories()]);
      setQuizzes(mine);
      setCategories(availableCategories);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to load your quizzes.');
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    let active = true;
    void Promise.all([quizzesService.mine(), quizzesService.categories()])
      .then(([mine, availableCategories]) => {
        if (!active) return;
        setQuizzes(mine);
        setCategories(availableCategories);
      })
      .catch((requestError: unknown) => {
        if (active) setError(requestError instanceof Error ? requestError.message : 'Unable to load your quizzes.');
      })
      .finally(() => { if (active) setLoading(false); });
    return () => { active = false; };
  }, []);

  function startCreating() {
    setEditingQuizId(null);
    setDraft(blankQuiz(categories[0]?.categoryId ?? 0));
    setError('');
  }

  async function startEditing(quizId: string) {
    setError('');
    try {
      const editor = await quizzesService.editor(quizId);
      setEditingQuizId(quizId);
      setDraft(toDraft(editor));
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to open this quiz.');
    }
  }

  function updateDraft(update: (current: QuizDraft) => QuizDraft) {
    setDraft((current) => current ? update(current) : current);
  }

  function updateQuestion(questionIndex: number, update: (question: QuestionDraft) => QuestionDraft) {
    updateDraft((current) => ({
      ...current,
      questions: current.questions.map((question, index) => index === questionIndex ? update(question) : question),
    }));
  }

  function addImage(image: ImageAsset) {
    if (!cropTarget) return;
    if (cropTarget.kind === 'cover') {
      updateDraft((current) => ({ ...current, coverImageId: image.imageAssetId }));
    } else if (cropTarget.kind === 'question') {
      updateQuestion(cropTarget.questionIndex, (question) => ({ ...question, imageId: image.imageAssetId }));
    } else {
      updateQuestion(cropTarget.questionIndex, (question) => ({
        ...question,
        answers: question.answers.map((answer, index) => index === cropTarget.answerIndex
          ? { ...answer, imageId: image.imageAssetId, text: '' }
          : answer),
      }));
    }
    setCropTarget(null);
  }

  function buildRequest(value: QuizDraft): QuizWriteRequest {
    return {
      title: value.title.trim(),
      categoryId: value.categoryId,
      description: value.description.trim(),
      coverImageId: value.coverImageId,
      questions: value.questions.map((question) => ({
        text: question.text.trim(),
        imageId: question.imageId,
        explanation: question.explanation.trim(),
        answers: question.answers.map((answer) => ({
          text: answer.imageId ? null : answer.text.trim(),
          imageId: answer.imageId,
          isCorrect: answer.isCorrect,
        })),
      })),
    };
  }

  async function saveQuiz(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!draft) return;
    if (!draft.title.trim() || !draft.categoryId || draft.questions.some((question) =>
      !question.text.trim()
      || question.answers.length < 2
      || question.answers.filter((answer) => answer.isCorrect).length !== 1
      || question.answers.some((answer) => !answer.imageId && !answer.text.trim()))) {
      setError('Add a title, choose a category, and give every question at least two answers with exactly one correct answer.');
      return;
    }
    setSaving(true);
    setError('');
    try {
      const request = buildRequest(draft);
      if (editingQuizId) {
        await quizzesService.update(editingQuizId, request);
        setNotice('Quiz updated and sent for administrator review.');
      } else {
        await quizzesService.create(request);
        setNotice('Quiz created and sent for administrator review.');
      }
      setDraft(null);
      await loadData();
    } catch (requestError) {
      setError(requestError instanceof ApiError
        ? requestError.message
        : requestError instanceof Error ? requestError.message : 'Unable to save this quiz.');
    } finally {
      setSaving(false);
    }
  }

  async function deleteQuiz(quiz: QuizSummary) {
    if (!window.confirm(`Delete "${quiz.title}"? This will also remove its attempt history.`)) return;
    setError('');
    try {
      await quizzesService.delete(quiz.quizId);
      setQuizzes((current) => current.filter((item) => item.quizId !== quiz.quizId));
      setNotice('Quiz deleted.');
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Unable to delete this quiz.');
    }
  }

  return (
    <section className="page-container quiz-manager-page">
      <div className="page-heading page-heading-row">
        <div>
          <span className="eyebrow">YOUR CREATIVE CORNER</span>
          <h1>Quiz Manager</h1>
          <p>Create, edit, and share quizzes that make people think.</p>
        </div>
        <button className="button button-primary" type="button" onClick={startCreating} disabled={categories.length === 0}>
          <CirclePlus size={18} aria-hidden="true" /> Create a quiz
        </button>
      </div>

      {notice && <div className="notice success-notice" role="status">{notice}<button type="button" className="notice-dismiss" aria-label="Dismiss" onClick={() => setNotice('')}><X size={16} /></button></div>}
      {error && <div className="notice error-notice" role="alert"><AlertCircle size={17} />{error}</div>}

      {categories.length === 0 && !loading && <div className="notice info-notice" role="status">There are no quiz categories yet. Ask an administrator to add categories before creating a quiz.</div>}

      {loading ? (
        <div className="collection-state" role="status"><LoaderCircle className="spin" size={24} /><p>Loading your quizzes…</p></div>
      ) : quizzes.length === 0 ? (
        <div className="manager-welcome">
          <div className="manager-illustration" aria-hidden="true"><span><PencilLine size={30} /></span><i /><i /><i /></div>
          <div className="manager-copy">
            <span className="eyebrow">YOUR NEXT GREAT QUIZ STARTS HERE</span>
            <h2>Ready to make learning memorable?</h2>
            <p>Create your first quiz, add questions and answers, then submit it for an administrator to review.</p>
            <button type="button" className="button button-primary" onClick={startCreating} disabled={categories.length === 0}><Plus size={17} /> Start creating</button>
          </div>
        </div>
      ) : (
        <div className="quiz-manager-grid">
          {quizzes.map((quiz) => (
            <article className="quiz-manager-card" key={quiz.quizId}>
              {quiz.coverImageId
                ? <img className="quiz-manager-cover" src={imageUrl(quiz.coverImageId)} alt="" />
                : <div className="quiz-manager-cover quiz-manager-cover-empty"><PencilLine size={24} aria-hidden="true" /></div>}
              <div className="quiz-manager-card-body">
                <div className="quiz-manager-card-meta"><span>{quiz.category}</span><span className={`quiz-status status-${quiz.status}`}>{statusNames[quiz.status] ?? 'Unknown status'}</span></div>
                <h2>{quiz.title}</h2>
                <p>{quiz.description || 'No description provided.'}</p>
                <span className="quiz-manager-count">{quiz.questionCount} questions</span>
                <div className="quiz-manager-actions">
                  <button type="button" className="button button-outline" onClick={() => void startEditing(quiz.quizId)}><PencilLine size={16} /> Edit</button>
                  <button type="button" className="icon-button danger-icon-button" aria-label={`Delete ${quiz.title}`} onClick={() => void deleteQuiz(quiz)}><Trash2 size={17} /></button>
                </div>
              </div>
            </article>
          ))}
        </div>
      )}

      {draft && (
        <div className="modal-backdrop quiz-editor-backdrop" onMouseDown={() => setDraft(null)}>
          <section className="quiz-editor-modal" role="dialog" aria-modal="true" aria-labelledby="quiz-editor-title" onMouseDown={(event) => event.stopPropagation()}>
            <div className="modal-heading quiz-editor-heading">
              <div><span className="eyebrow">QUIZ BUILDER</span><h2 id="quiz-editor-title">{editingQuizId ? 'Edit your quiz' : 'Create a quiz'}</h2><p>Changes are reviewed by an administrator before the quiz is published.</p></div>
              <button type="button" className="icon-button" aria-label="Close quiz editor" onClick={() => setDraft(null)}><X size={20} /></button>
            </div>
            {error && <div className="notice error-notice" role="alert">{error}</div>}
            <form className="quiz-editor-form" onSubmit={(event) => void saveQuiz(event)}>
              <div className="quiz-basic-fields">
                <div className="form-field"><label htmlFor="quiz-title">Quiz title</label><input id="quiz-title" maxLength={200} value={draft.title} onChange={(event) => updateDraft((current) => ({ ...current, title: event.target.value }))} required /></div>
                <div className="form-field"><label htmlFor="quiz-category">Category</label><select id="quiz-category" value={draft.categoryId} onChange={(event) => updateDraft((current) => ({ ...current, categoryId: Number(event.target.value) }))} required><option value={0} disabled>Select a category</option>{categories.map((category) => <option value={category.categoryId} key={category.categoryId}>{category.name}</option>)}</select></div>
                <div className="form-field quiz-description-field"><label htmlFor="quiz-description">Description <span className="optional-label">Optional</span></label><textarea id="quiz-description" maxLength={2000} rows={3} value={draft.description} onChange={(event) => updateDraft((current) => ({ ...current, description: event.target.value }))} /></div>
              </div>

              <div className="quiz-image-field">
                <div><strong>Cover image</strong><span>Square image, cropped before upload</span></div>
                {draft.coverImageId && <img src={imageUrl(draft.coverImageId)} alt="Quiz cover preview" />}
                <button type="button" className="button button-outline" onClick={() => setCropTarget({ kind: 'cover' })}><ImagePlus size={16} />{draft.coverImageId ? 'Change image' : 'Add image'}</button>
              </div>

              <div className="quiz-questions-heading"><div><h3>Questions</h3><p>Add clear prompts and at least two answer options.</p></div><span>{draft.questions.length}</span></div>
              <div className="quiz-question-list">
                {draft.questions.map((question, questionIndex) => (
                  <fieldset className="quiz-question-card" key={questionIndex}>
                    <legend>Question {questionIndex + 1}</legend>
                    <div className="quiz-question-topline">
                      <span className="quiz-question-number">{String(questionIndex + 1).padStart(2, '0')}</span>
                      {draft.questions.length > 1 && <button type="button" className="icon-button danger-icon-button" aria-label={`Remove question ${questionIndex + 1}`} onClick={() => updateDraft((current) => ({ ...current, questions: current.questions.filter((_, index) => index !== questionIndex) }))}><Trash2 size={16} /></button>}
                    </div>
                    <div className="form-field"><label htmlFor={`question-${questionIndex}`}>Question text</label><textarea id={`question-${questionIndex}`} maxLength={2000} rows={2} value={question.text} onChange={(event) => updateQuestion(questionIndex, (current) => ({ ...current, text: event.target.value }))} required /></div>
                    <div className="quiz-question-image-row">
                      {question.imageId && <img src={imageUrl(question.imageId)} alt="Question preview" />}
                      <button type="button" className="text-button" onClick={() => setCropTarget({ kind: 'question', questionIndex })}><ImagePlus size={15} />{question.imageId ? 'Change question image' : 'Add question image'}</button>
                      {question.imageId && <button type="button" className="text-button text-button-danger" onClick={() => updateQuestion(questionIndex, (current) => ({ ...current, imageId: null }))}>Remove image</button>}
                    </div>

                    <div className="quiz-answers-heading"><strong>Answers</strong><span>Select the one correct answer.</span></div>
                    <div className="quiz-answer-list">
                      {question.answers.map((answer, answerIndex) => (
                        <div className={`quiz-answer-row${answer.isCorrect ? ' is-correct-answer' : ''}`} key={answerIndex}>
                          <label className="correct-answer-toggle" title="Mark as correct">
                            <input type="radio" name={`correct-answer-${questionIndex}`} checked={answer.isCorrect} onChange={() => updateQuestion(questionIndex, (current) => ({ ...current, answers: current.answers.map((option, index) => ({ ...option, isCorrect: index === answerIndex })) }))} />
                            <Check size={15} aria-hidden="true" />
                          </label>
                          {answer.imageId
                            ? <img className="quiz-answer-preview" src={imageUrl(answer.imageId)} alt={`Answer ${answerIndex + 1}`} />
                            : <input className="quiz-answer-input" aria-label={`Answer ${answerIndex + 1}`} maxLength={1000} placeholder={`Answer ${answerIndex + 1}`} value={answer.text} onChange={(event) => updateQuestion(questionIndex, (current) => ({ ...current, answers: current.answers.map((option, index) => index === answerIndex ? { ...option, text: event.target.value } : option) }))} />}
                          <button type="button" className="icon-button answer-image-button" aria-label={`Add image to answer ${answerIndex + 1}`} title="Add image answer" onClick={() => setCropTarget({ kind: 'answer', questionIndex, answerIndex })}><ImagePlus size={16} /></button>
                          {answer.imageId && <button type="button" className="icon-button" aria-label={`Remove image from answer ${answerIndex + 1}`} onClick={() => updateQuestion(questionIndex, (current) => ({ ...current, answers: current.answers.map((option, index) => index === answerIndex ? { ...option, imageId: null } : option) }))}><X size={16} /></button>}
                          {question.answers.length > 2 && <button type="button" className="icon-button danger-icon-button" aria-label={`Remove answer ${answerIndex + 1}`} onClick={() => updateQuestion(questionIndex, (current) => {
                            const answers = current.answers.filter((_, index) => index !== answerIndex);
                            if (!answers.some((option) => option.isCorrect)) answers[0] = { ...answers[0], isCorrect: true };
                            return { ...current, answers };
                          })}><Trash2 size={15} /></button>}
                        </div>
                      ))}
                    </div>
                    <button type="button" className="add-answer-button" onClick={() => updateQuestion(questionIndex, (current) => ({ ...current, answers: [...current.answers, { text: '', imageId: null, isCorrect: false }] }))}><Plus size={15} /> Add answer</button>
                    <div className="form-field quiz-explanation-field"><label htmlFor={`explanation-${questionIndex}`}>Explanation <span className="optional-label">Optional</span></label><textarea id={`explanation-${questionIndex}`} maxLength={2000} rows={2} value={question.explanation} onChange={(event) => updateQuestion(questionIndex, (current) => ({ ...current, explanation: event.target.value }))} /></div>
                  </fieldset>
                ))}
              </div>
              <button type="button" className="add-question-button" onClick={() => updateDraft((current) => ({ ...current, questions: [...current.questions, blankQuestion()] }))}><CirclePlus size={18} /> Add question</button>

              <div className="modal-actions quiz-editor-actions">
                <button type="button" className="button button-outline" onClick={() => setDraft(null)} disabled={saving}>Cancel</button>
                <button type="submit" className="button button-primary" disabled={saving || categories.length === 0}>{saving ? <LoaderCircle className="spin" size={17} /> : <Check size={17} />}{saving ? 'Saving…' : editingQuizId ? 'Save changes' : 'Submit for review'}</button>
              </div>
            </form>
          </section>
        </div>
      )}

      {cropTarget && <ImageCropDialog onClose={() => setCropTarget(null)} onUploaded={addImage} />}
    </section>
  );
}
