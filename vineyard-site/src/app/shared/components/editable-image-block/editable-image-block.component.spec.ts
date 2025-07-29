import { ComponentFixture, TestBed } from '@angular/core/testing';
import { EditableImageBlockComponent } from './editable-image-block.component';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

describe('EditableImageBlockComponent', () => {
  let component: EditableImageBlockComponent;
  let fixture: ComponentFixture<EditableImageBlockComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CommonModule, FormsModule, EditableImageBlockComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(EditableImageBlockComponent);
    component = fixture.componentInstance;
    component.editable = true;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('binds initial src and alt inputs', async () => {
    component.src = 'test.jpg';
    component.alt = 'desc';
    fixture.detectChanges();
    await fixture.whenStable();
    const img = fixture.nativeElement.querySelector('img') as HTMLImageElement;
    const textInput = fixture.nativeElement.querySelector('input[type="text"]') as HTMLInputElement;
    expect(img.getAttribute('src')).toContain('test.jpg');
    expect(img.alt).toBe('desc');
    expect(textInput.value).toBe('test.jpg');
  });

  it('updates src on input change and emits value', async () => {
    const input = fixture.nativeElement.querySelector('input[type="text"]') as HTMLInputElement;
    let emitted: string | undefined;
    component.srcChange.subscribe(v => emitted = v);
    input.value = 'updated.jpg';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();
    await fixture.whenStable();
    expect(component.src).toBe('updated.jpg');
    expect(emitted).toBe('updated.jpg');
  });
});
