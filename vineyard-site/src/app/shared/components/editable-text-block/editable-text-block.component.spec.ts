import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditableTextBlockComponent } from './editable-text-block.component';
import { FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';
import { By } from '@angular/platform-browser';

describe('EditableTextBlockComponent', () => {
  let component: EditableTextBlockComponent;
  let fixture: ComponentFixture<EditableTextBlockComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FormsModule, QuillModule.forRoot(), EditableTextBlockComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(EditableTextBlockComponent);
    component = fixture.componentInstance;
    component.editable = true;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('binds initial content value', () => {
    component.content = 'initial';
    fixture.detectChanges();
    const el = fixture.nativeElement.querySelector('quill-editor');
    expect(el.getAttribute('ng-reflect-model')).toContain('initial');
  });

  it('updates content on ngModelChange and emits value', () => {
    const editor = fixture.debugElement.query(By.css('quill-editor'));
    let emitted: string | undefined;
    component.contentChange.subscribe(v => emitted = v);
    editor.triggerEventHandler('ngModelChange', 'updated');
    fixture.detectChanges();
    expect(component.content).toBe('updated');
    expect(emitted).toBe('updated');
  });
});
