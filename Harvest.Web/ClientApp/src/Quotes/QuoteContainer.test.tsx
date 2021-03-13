import React from 'react';
import { render, unmountComponentAtNode } from 'react-dom';
import { MemoryRouter, Route } from 'react-router-dom';
import { act } from 'react-dom/test-utils';

import { QuoteContainer } from './QuoteContainer';

let container: Element = null;

beforeEach(() => {
  // setup a DOM element as a render target
  container = document.createElement('div');
  document.body.appendChild(container);
});

afterEach(() => {
  // cleanup on exiting
  unmountComponentAtNode(container); 
  container.remove();
  container = null;
});

describe('Quote Container', () => {
  it('Shows Create quote message', async () => {
    await act(async () => {
      render(
        <MemoryRouter initialEntries={['/quote/create/1']}>
          <Route path="/quote/create/:projectId">
            <QuoteContainer />
          </Route>
        </MemoryRouter>,
        container
      );
    });

    const messageContent = container.querySelector('h3').textContent;
    expect(messageContent).toContain('Create quote for project: 1');
  });

});

